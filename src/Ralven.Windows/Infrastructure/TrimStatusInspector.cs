using System.ComponentModel;

namespace Ralven.Windows.Infrastructure;

public enum TrimInspectionState
{
    Available,
    Partial,
    Unsupported,
    AccessDenied,
    Unavailable
}

public enum TrimDeleteNotificationState
{
    Enabled = 0,
    Disabled = 1
}

public sealed record TrimStatusSnapshot(
    TrimInspectionState State,
    TrimDeleteNotificationState? Ntfs,
    TrimDeleteNotificationState? ReFs);

public interface ITrimStatusInspector
{
    Task<TrimStatusSnapshot> InspectAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Queries the documented DisableDeleteNotify state through a fixed, read-only
/// fsutil invocation. Parsing uses only the filesystem identifier and numeric
/// value, so localized explanatory labels do not affect the result.
/// </summary>
public sealed class WindowsTrimStatusInspector : ITrimStatusInspector
{
    private const int ErrorAccessDenied = 5;
    private const int ErrorNotSupported = 50;
    private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(10);
    private static readonly string FsutilPath = Path.Combine(Environment.SystemDirectory, "fsutil.exe");
    private static readonly string[] QueryArguments = ["behavior", "query", "DisableDeleteNotify"];

    private readonly ICommandRunner runner;

    public WindowsTrimStatusInspector()
        : this(new ProcessCommandRunner())
    {
    }

    internal WindowsTrimStatusInspector(ICommandRunner runner)
    {
        this.runner = runner ?? throw new ArgumentNullException(nameof(runner));
    }

    public async Task<TrimStatusSnapshot> InspectAsync(
        CancellationToken cancellationToken = default)
    {
        CommandResult result;
        try
        {
            result = await runner.RunAsync(
                FsutilPath,
                QueryArguments,
                QueryTimeout,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            return Empty(TrimInspectionState.AccessDenied);
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode == ErrorAccessDenied)
        {
            return Empty(TrimInspectionState.AccessDenied);
        }
        catch (Exception exception) when (exception is PlatformNotSupportedException
            or NotSupportedException
            || exception is Win32Exception { NativeErrorCode: ErrorNotSupported })
        {
            return Empty(TrimInspectionState.Unsupported);
        }
        catch (Exception exception) when (exception is not (
            OutOfMemoryException or StackOverflowException or AccessViolationException))
        {
            return Empty(TrimInspectionState.Unavailable);
        }

        if (!result.Succeeded)
        {
            return Empty(result.ExitCode switch
            {
                ErrorAccessDenied => TrimInspectionState.AccessDenied,
                ErrorNotSupported => TrimInspectionState.Unsupported,
                _ => TrimInspectionState.Unavailable
            });
        }

        var ntfs = ParseState(result.StandardOutput, "NTFS");
        var refs = ParseState(result.StandardOutput, "ReFS");
        var state = (ntfs, refs) switch
        {
            (not null, not null) => TrimInspectionState.Available,
            (not null, null) or (null, not null) => TrimInspectionState.Partial,
            _ => TrimInspectionState.Unavailable
        };

        return new TrimStatusSnapshot(state, ntfs, refs);
    }

    private static TrimStatusSnapshot Empty(TrimInspectionState state) =>
        new(state, null, null);

    private static TrimDeleteNotificationState? ParseState(string output, string fileSystem)
    {
        foreach (var line in output.AsSpan().EnumerateLines())
        {
            var value = line.Trim();
            if (!value.StartsWith(fileSystem, StringComparison.OrdinalIgnoreCase)
                || (value.Length > fileSystem.Length
                    && !char.IsWhiteSpace(value[fileSystem.Length])))
            {
                continue;
            }

            var equalsIndex = value.IndexOf('=');
            if (equalsIndex < 0)
            {
                continue;
            }

            var numericValue = value[(equalsIndex + 1)..].TrimStart();
            if (numericValue.IsEmpty
                || numericValue[0] is < '0' or > '1'
                || (numericValue.Length > 1 && !char.IsWhiteSpace(numericValue[1])))
            {
                continue;
            }

            return (TrimDeleteNotificationState)(numericValue[0] - '0');
        }

        return null;
    }
}
