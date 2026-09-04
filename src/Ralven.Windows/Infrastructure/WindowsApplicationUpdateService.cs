using System.Text.RegularExpressions;

namespace Ralven.Windows.Infrastructure;

public sealed record WindowsApplicationUpdate(
    string PackageId,
    string Name,
    string InstalledVersion,
    string AvailableVersion,
    string Source);

public sealed record WindowsApplicationUpdateSnapshot(
    IReadOnlyList<WindowsApplicationUpdate> Updates,
    DateTimeOffset ObservedAtUtc,
    bool IsWinGetAvailable);

public enum WindowsApplicationUpdateOutcome
{
    Succeeded,
    NoLongerAvailable,
    WinGetUnavailable,
    Failed
}

public sealed record WindowsApplicationUpdateResult(
    WindowsApplicationUpdateOutcome Outcome,
    int? ExitCode = null);

public interface IWindowsApplicationUpdateService
{
    Task<WindowsApplicationUpdateSnapshot> CheckAsync(
        CancellationToken cancellationToken = default);

    Task<WindowsApplicationUpdateResult> UpdateAsync(
        WindowsApplicationUpdate update,
        CancellationToken cancellationToken = default);
}

public sealed partial class WinGetApplicationUpdateService : IWindowsApplicationUpdateService
{
    private const int NoApplicationsFoundExitCode = unchecked((int)0x8A150014);
    private const int UpdateNotApplicableExitCode = unchecked((int)0x8A15002B);
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan UpdateTimeout = TimeSpan.FromHours(2);
    private readonly ICommandRunner commandRunner;
    private readonly string? winGetExecutable;

    public WinGetApplicationUpdateService()
        : this(new ProcessCommandRunner(), FindWinGetExecutable())
    {
    }

    internal WinGetApplicationUpdateService(
        ICommandRunner commandRunner,
        string? winGetExecutable)
    {
        this.commandRunner = commandRunner ?? throw new ArgumentNullException(nameof(commandRunner));
        if (winGetExecutable is not null
            && (!Path.IsPathFullyQualified(winGetExecutable)
                || !string.Equals(
                    Path.GetFileName(winGetExecutable),
                    "winget.exe",
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException(
                "WinGet must use an absolute path to winget.exe.",
                nameof(winGetExecutable));
        }

        this.winGetExecutable = winGetExecutable;
    }

    public async Task<WindowsApplicationUpdateSnapshot> CheckAsync(
        CancellationToken cancellationToken = default)
    {
        if (winGetExecutable is null)
        {
            return new WindowsApplicationUpdateSnapshot([], DateTimeOffset.UtcNow, false);
        }

        var result = await commandRunner.RunAsync(
            winGetExecutable,
            [
                "list",
                "--upgrade-available",
                "--source",
                "winget",
                "--sort",
                "name",
                "--ascending",
                "--disable-interactivity"
            ],
            CheckTimeout,
            cancellationToken).ConfigureAwait(false);

        if (result.ExitCode == NoApplicationsFoundExitCode)
        {
            return new WindowsApplicationUpdateSnapshot([], DateTimeOffset.UtcNow, true);
        }

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"WinGet could not list updates (exit 0x{result.ExitCode:X8}).");
        }

        return new WindowsApplicationUpdateSnapshot(
            ParseUpdates(result.StandardOutput),
            DateTimeOffset.UtcNow,
            true);
    }

    public async Task<WindowsApplicationUpdateResult> UpdateAsync(
        WindowsApplicationUpdate update,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        if (!PackageIdPattern().IsMatch(update.PackageId)
            || !string.Equals(update.Source, "winget", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The update is not a valid WinGet package.", nameof(update));
        }

        if (winGetExecutable is null)
        {
            return new WindowsApplicationUpdateResult(
                WindowsApplicationUpdateOutcome.WinGetUnavailable);
        }

        var result = await commandRunner.RunAsync(
            winGetExecutable,
            [
                "upgrade",
                "--id",
                update.PackageId,
                "--exact",
                "--source",
                "winget",
                "--accept-package-agreements",
                "--accept-source-agreements",
                "--disable-interactivity"
            ],
            UpdateTimeout,
            cancellationToken).ConfigureAwait(false);

        return new WindowsApplicationUpdateResult(
            result.ExitCode switch
            {
                0 => WindowsApplicationUpdateOutcome.Succeeded,
                NoApplicationsFoundExitCode or UpdateNotApplicableExitCode =>
                    WindowsApplicationUpdateOutcome.NoLongerAvailable,
                _ => WindowsApplicationUpdateOutcome.Failed
            },
            result.ExitCode);
    }

    internal static IReadOnlyList<WindowsApplicationUpdate> ParseUpdates(string output)
    {
        ArgumentNullException.ThrowIfNull(output);
        var lines = AnsiEscapePattern()
            .Replace(output, string.Empty)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var separatorIndex = -1;
        int[]? columnStarts = null;
        for (var index = 1; index < lines.Length; index++)
        {
            if (!TableSeparatorPattern().IsMatch(lines[index]))
            {
                continue;
            }

            separatorIndex = index;
            columnStarts =
            [
                0,
                .. HeaderGapPattern()
                    .Matches(lines[index - 1])
                    .Select(gap => gap.Index + gap.Length)
            ];
        }

        if (separatorIndex < 0 || columnStarts is null)
        {
            return [];
        }

        if (columnStarts.Length is < 4 or > 5)
        {
            throw new InvalidDataException("WinGet returned an unsupported list format.");
        }

        var updates = new List<WindowsApplicationUpdate>();
        foreach (var line in lines.Skip(separatorIndex + 1))
        {
            var name = ReadColumn(line, columnStarts, 0);
            var packageId = ReadColumn(line, columnStarts, 1);
            var installedVersion = ReadColumn(line, columnStarts, 2);
            var availableVersion = ReadColumn(line, columnStarts, 3);
            var source = columnStarts.Length == 5
                ? ReadColumn(line, columnStarts, 4)
                : "winget";
            if (name.Length == 0
                || installedVersion.Length == 0
                || availableVersion.Length == 0
                || packageId.Contains('…', StringComparison.Ordinal)
                || !PackageIdPattern().IsMatch(packageId)
                || !string.Equals(source, "winget", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            updates.Add(new WindowsApplicationUpdate(
                packageId,
                name,
                installedVersion,
                availableVersion,
                source));
        }

        return updates;
    }

    private static string ReadColumn(string line, int[] columnStarts, int column)
    {
        var start = columnStarts[column];
        if (start >= line.Length)
        {
            return string.Empty;
        }

        var end = column == columnStarts.Length - 1
            ? line.Length
            : Math.Min(columnStarts[column + 1], line.Length);
        return line[start..end].Trim();
    }

    private static string? FindWinGetExecutable()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
        {
            return null;
        }

        var windowsApps = Path.GetFullPath(Path.Combine(localAppData, "Microsoft", "WindowsApps"));
        var candidate = Path.GetFullPath(Path.Combine(windowsApps, "winget.exe"));
        return File.Exists(candidate) ? candidate : null;
    }

    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9._+\-]{0,255}$", RegexOptions.CultureInvariant)]
    private static partial Regex PackageIdPattern();

    [GeneratedRegex(@"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])", RegexOptions.CultureInvariant)]
    private static partial Regex AnsiEscapePattern();

    [GeneratedRegex(@" {2,}", RegexOptions.CultureInvariant)]
    private static partial Regex HeaderGapPattern();

    [GeneratedRegex(@"^-{10,}$", RegexOptions.CultureInvariant)]
    private static partial Regex TableSeparatorPattern();
}
