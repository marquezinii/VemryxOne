using System.Security;
using Microsoft.Win32;

namespace Ralven.Windows.Infrastructure;

public interface IWindowsApplicationInventoryInspector
{
    Task<WindowsApplicationInventorySnapshot> InspectAsync(
        CancellationToken cancellationToken = default);
}

public enum WindowsApplicationScope
{
    CurrentUser,
    LocalMachine
}

public enum WindowsApplicationArchitecture
{
    Unknown,
    X86,
    X64
}

public enum WindowsStartupItemSource
{
    RegistryRun,
    RegistryRunOnce,
    StartupFolder
}

public sealed record WindowsInstalledApplication(
    string DisplayName,
    string? DisplayVersion,
    string? Publisher,
    long? EstimatedSizeBytes,
    WindowsApplicationScope Scope,
    WindowsApplicationArchitecture Architecture);

public sealed record WindowsStartupItem(
    string Name,
    string Location,
    WindowsStartupItemSource Source,
    WindowsApplicationScope Scope);

public sealed record WindowsApplicationInventorySnapshot(
    IReadOnlyList<WindowsInstalledApplication> InstalledApplications,
    IReadOnlyList<WindowsStartupItem> StartupItems,
    DateTimeOffset ObservedAtUtc,
    bool InstalledApplicationsComplete,
    bool StartupItemsComplete)
{
    public bool IsPartial => !InstalledApplicationsComplete || !StartupItemsComplete;
}

public sealed class WindowsApplicationInventoryInspector : IWindowsApplicationInventoryInspector
{
    private const string UninstallSubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    private const string RunSubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string RunOnceSubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce";
    private const int ShortTextLimit = 256;
    private const long MaxEstimatedSizeBytes = 16L * 1024 * 1024 * 1024 * 1024;

    private readonly Func<CancellationToken, WindowsApplicationInventoryReadResult> readInventory;
    private readonly Func<DateTimeOffset> utcNow;

    public WindowsApplicationInventoryInspector()
        : this(ReadInventory, static () => DateTimeOffset.UtcNow)
    {
    }

    internal WindowsApplicationInventoryInspector(
        Func<CancellationToken, WindowsApplicationInventoryReadResult> readInventory,
        Func<DateTimeOffset>? utcNow = null)
    {
        this.readInventory = readInventory ?? throw new ArgumentNullException(nameof(readInventory));
        this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public Task<WindowsApplicationInventorySnapshot> InspectAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.Run(
            () => BuildSnapshot(readInventory(cancellationToken), cancellationToken),
            cancellationToken);
    }

    internal static long? ConvertEstimatedSizeToBytes(long? estimatedSizeKib)
    {
        if (estimatedSizeKib is null or <= 0
            || estimatedSizeKib > MaxEstimatedSizeBytes / 1024)
        {
            return null;
        }

        return estimatedSizeKib * 1024;
    }

    private WindowsApplicationInventorySnapshot BuildSnapshot(
        WindowsApplicationInventoryReadResult result,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var applications = result.InstalledApplications
            .Select(entry =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Normalize(entry);
            })
            .Where(static application => application is not null)
            .Cast<WindowsInstalledApplication>()
            .GroupBy(
                static application => string.Join(
                    '\u001f',
                    application.DisplayName,
                    application.DisplayVersion ?? string.Empty,
                    application.Publisher ?? string.Empty,
                    application.Scope),
                StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .OrderBy(static application => application.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(static application => application.DisplayVersion, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        cancellationToken.ThrowIfCancellationRequested();

        var startupItems = result.StartupItems
            .Select(entry =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Normalize(entry);
            })
            .Where(static item => item is not null)
            .Cast<WindowsStartupItem>()
            .DistinctBy(
                static item => string.Join(
                    '\u001f',
                    item.Name,
                    item.Location,
                    item.Source,
                    item.Scope),
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(static item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(static item => item.Location, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        return new WindowsApplicationInventorySnapshot(
            applications,
            startupItems,
            utcNow(),
            result.InstalledApplicationsComplete,
            result.StartupItemsComplete);
    }

    private static WindowsInstalledApplication? Normalize(WindowsApplicationInventoryEntry entry)
    {
        var displayName = NormalizeText(entry.DisplayName, ShortTextLimit);
        if (displayName is null)
        {
            return null;
        }

        return new WindowsInstalledApplication(
            displayName,
            NormalizeText(entry.DisplayVersion, ShortTextLimit),
            NormalizeText(entry.Publisher, ShortTextLimit),
            ConvertEstimatedSizeToBytes(entry.EstimatedSizeKib),
            entry.Scope,
            entry.Architecture);
    }

    private static WindowsStartupItem? Normalize(WindowsStartupInventoryEntry entry)
    {
        var name = NormalizeText(entry.Name, ShortTextLimit);
        var location = NormalizeText(entry.Location, ShortTextLimit);
        if (name is null || location is null)
        {
            return null;
        }

        return new WindowsStartupItem(name, location, entry.Source, entry.Scope);
    }

    private static string? NormalizeText(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = string.Join(
            ' ',
            value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= maximumLength
            ? normalized
            : normalized[..maximumLength];
    }

    private static WindowsApplicationInventoryReadResult ReadInventory(
        CancellationToken cancellationToken)
    {
        var (applications, applicationsComplete) = ReadInstalledApplications(cancellationToken);
        var (startupItems, startupItemsComplete) = ReadStartupItems(cancellationToken);
        return new WindowsApplicationInventoryReadResult(
            applications,
            startupItems,
            applicationsComplete,
            startupItemsComplete);
    }

    private static (IReadOnlyList<WindowsApplicationInventoryEntry> Items, bool Complete)
        ReadInstalledApplications(CancellationToken cancellationToken)
    {
        var items = new List<WindowsApplicationInventoryEntry>();
        var complete = true;

        foreach (var (hive, view, scope, architecture) in UninstallLocations())
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                using var uninstallKey = baseKey.OpenSubKey(UninstallSubKey, writable: false);
                if (uninstallKey is null)
                {
                    continue;
                }

                foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        using var applicationKey = uninstallKey.OpenSubKey(subKeyName, writable: false);
                        if (applicationKey is null || IsHiddenOrUpdate(applicationKey))
                        {
                            continue;
                        }

                        var displayName = ReadString(applicationKey, "DisplayName");
                        if (string.IsNullOrWhiteSpace(displayName))
                        {
                            continue;
                        }

                        items.Add(new WindowsApplicationInventoryEntry(
                            displayName,
                            ReadString(applicationKey, "DisplayVersion"),
                            ReadString(applicationKey, "Publisher"),
                            ReadUnsignedDword(applicationKey, "EstimatedSize"),
                            scope,
                            architecture));
                    }
                    catch (Exception exception) when (IsPartialFailure(exception))
                    {
                        complete = false;
                    }
                }
            }
            catch (Exception exception) when (IsPartialFailure(exception))
            {
                complete = false;
            }
        }

        return (items, complete);
    }

    private static (IReadOnlyList<WindowsStartupInventoryEntry> Items, bool Complete)
        ReadStartupItems(CancellationToken cancellationToken)
    {
        var items = new List<WindowsStartupInventoryEntry>();
        var complete = true;

        foreach (var (hive, view, scope) in StartupRegistryLocations())
        {
            foreach (var (subKey, source) in new[]
            {
                (RunSubKey, WindowsStartupItemSource.RegistryRun),
                (RunOnceSubKey, WindowsStartupItemSource.RegistryRunOnce)
            })
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                    using var startupKey = baseKey.OpenSubKey(subKey, writable: false);
                    if (startupKey is null)
                    {
                        continue;
                    }

                    var location = $"{scope}:{source}";
                    foreach (var valueName in startupKey.GetValueNames())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        items.Add(new WindowsStartupInventoryEntry(valueName, location, source, scope));
                    }
                }
                catch (Exception exception) when (IsPartialFailure(exception))
                {
                    complete = false;
                }
            }
        }

        foreach (var (folder, scope, location) in StartupFolders())
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (string.IsNullOrWhiteSpace(folder))
                {
                    continue;
                }

                foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    items.Add(new WindowsStartupInventoryEntry(
                        Path.GetFileNameWithoutExtension(file),
                        location,
                        WindowsStartupItemSource.StartupFolder,
                        scope));
                }
            }
            catch (DirectoryNotFoundException)
            {
                // An optional startup folder may not have been created yet.
            }
            catch (Exception exception) when (IsPartialFailure(exception))
            {
                complete = false;
            }
        }

        return (items, complete);
    }

    private static bool IsHiddenOrUpdate(RegistryKey key)
    {
        if (ReadUnsignedDword(key, "SystemComponent") == 1
            || !string.IsNullOrWhiteSpace(ReadString(key, "ParentKeyName")))
        {
            return true;
        }

        var releaseType = ReadString(key, "ReleaseType");
        return releaseType?.Contains("update", StringComparison.OrdinalIgnoreCase) == true
            || releaseType?.Contains("hotfix", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string? ReadString(RegistryKey key, string valueName)
    {
        if (!key.GetValueNames().Contains(valueName, StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        var kind = key.GetValueKind(valueName);
        if (kind is not (RegistryValueKind.String or RegistryValueKind.ExpandString))
        {
            return null;
        }

        return key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
    }

    private static long? ReadUnsignedDword(RegistryKey key, string valueName)
    {
        if (!key.GetValueNames().Contains(valueName, StringComparer.OrdinalIgnoreCase)
            || key.GetValueKind(valueName) != RegistryValueKind.DWord)
        {
            return null;
        }

        return key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames) is int value
            ? unchecked((uint)value)
            : null;
    }

    private static bool IsPartialFailure(Exception exception)
    {
        return exception is IOException or UnauthorizedAccessException or SecurityException;
    }

    private static IEnumerable<(
        RegistryHive Hive,
        RegistryView View,
        WindowsApplicationScope Scope,
        WindowsApplicationArchitecture Architecture)> UninstallLocations()
    {
        yield return (RegistryHive.CurrentUser, RegistryView.Registry64,
            WindowsApplicationScope.CurrentUser, WindowsApplicationArchitecture.X64);
        yield return (RegistryHive.CurrentUser, RegistryView.Registry32,
            WindowsApplicationScope.CurrentUser, WindowsApplicationArchitecture.X86);
        yield return (RegistryHive.LocalMachine, RegistryView.Registry64,
            WindowsApplicationScope.LocalMachine, WindowsApplicationArchitecture.X64);
        yield return (RegistryHive.LocalMachine, RegistryView.Registry32,
            WindowsApplicationScope.LocalMachine, WindowsApplicationArchitecture.X86);
    }

    private static IEnumerable<(
        RegistryHive Hive,
        RegistryView View,
        WindowsApplicationScope Scope)> StartupRegistryLocations()
    {
        yield return (RegistryHive.CurrentUser, RegistryView.Registry64, WindowsApplicationScope.CurrentUser);
        yield return (RegistryHive.CurrentUser, RegistryView.Registry32, WindowsApplicationScope.CurrentUser);
        yield return (RegistryHive.LocalMachine, RegistryView.Registry64, WindowsApplicationScope.LocalMachine);
        yield return (RegistryHive.LocalMachine, RegistryView.Registry32, WindowsApplicationScope.LocalMachine);
    }

    private static IEnumerable<(string Folder, WindowsApplicationScope Scope, string Location)>
        StartupFolders()
    {
        yield return (
            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
            WindowsApplicationScope.CurrentUser,
            "CurrentUser:StartupFolder");
        yield return (
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup),
            WindowsApplicationScope.LocalMachine,
            "LocalMachine:StartupFolder");
    }
}

internal sealed record WindowsApplicationInventoryEntry(
    string? DisplayName,
    string? DisplayVersion,
    string? Publisher,
    long? EstimatedSizeKib,
    WindowsApplicationScope Scope,
    WindowsApplicationArchitecture Architecture);

internal sealed record WindowsStartupInventoryEntry(
    string? Name,
    string? Location,
    WindowsStartupItemSource Source,
    WindowsApplicationScope Scope);

internal sealed record WindowsApplicationInventoryReadResult(
    IReadOnlyList<WindowsApplicationInventoryEntry> InstalledApplications,
    IReadOnlyList<WindowsStartupInventoryEntry> StartupItems,
    bool InstalledApplicationsComplete,
    bool StartupItemsComplete);
