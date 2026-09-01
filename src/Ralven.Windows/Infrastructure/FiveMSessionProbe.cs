using System.ComponentModel;
using System.Diagnostics;
using System.Security;

namespace Ralven.Windows.Infrastructure;

public enum FiveMSessionPresence
{
    Present,
    AbsentConfirmed,
    Indeterminate
}

public static class WindowsFiveMSessionProbe
{
    public static FiveMSessionPresence Probe(string legacyRoot)
    {
        if (!TryValidateLegacyRoot(legacyRoot, out var normalizedRoot))
        {
            return FiveMSessionPresence.Indeterminate;
        }

        Process[] processes;
        try
        {
            processes = Process.GetProcesses();
        }
        catch (Exception exception) when (IsProcessInspectionFailure(exception))
        {
            return FiveMSessionPresence.Indeterminate;
        }

        var result = FiveMSessionPresence.AbsentConfirmed;
        try
        {
            foreach (var process in processes)
            {
                var candidate = InspectProcess(process, normalizedRoot);
                if (candidate == FiveMSessionPresence.Present)
                {
                    result = FiveMSessionPresence.Present;
                    break;
                }
                else if (candidate == FiveMSessionPresence.Indeterminate
                    && result == FiveMSessionPresence.AbsentConfirmed)
                {
                    result = FiveMSessionPresence.Indeterminate;
                }
            }
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }

        return result;
    }

    internal static FiveMSessionPresence ClassifyCandidate(
        string? processName,
        string? executablePath,
        string legacyRoot)
    {
        if (!TryValidateLegacyRoot(legacyRoot, out var normalizedRoot))
        {
            return FiveMSessionPresence.Indeterminate;
        }

        return ClassifyValidatedCandidate(processName, executablePath, normalizedRoot);
    }

    private static FiveMSessionPresence InspectProcess(Process process, string normalizedRoot)
    {
        string processName;
        try
        {
            processName = process.ProcessName;
        }
        catch (Exception exception) when (IsProcessInspectionFailure(exception))
        {
            return FiveMSessionPresence.Indeterminate;
        }

        if (string.IsNullOrWhiteSpace(processName))
        {
            return FiveMSessionPresence.Indeterminate;
        }

        if (!WindowsFiveMProcessInspector.LooksLikeFiveMProcessName(processName))
        {
            return FiveMSessionPresence.AbsentConfirmed;
        }

        string? executablePath;
        try
        {
            executablePath = process.MainModule?.FileName;
        }
        catch (Exception exception) when (IsProcessInspectionFailure(exception))
        {
            return FiveMSessionPresence.Indeterminate;
        }

        return ClassifyValidatedCandidate(processName, executablePath, normalizedRoot);
    }

    private static FiveMSessionPresence ClassifyValidatedCandidate(
        string? processName,
        string? executablePath,
        string normalizedRoot)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return FiveMSessionPresence.Indeterminate;
        }

        if (!WindowsFiveMProcessInspector.LooksLikeFiveMProcessName(processName))
        {
            return FiveMSessionPresence.AbsentConfirmed;
        }

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return FiveMSessionPresence.Indeterminate;
        }

        try
        {
            var normalizedImage = SafePath.Normalize(executablePath);
            var imageLeaf = Path.GetFileName(normalizedImage);
            if (!IsAllowedExecutableLeaf(imageLeaf)
                || !IsDescendant(normalizedRoot, normalizedImage))
            {
                return FiveMSessionPresence.AbsentConfirmed;
            }

            if (!File.Exists(normalizedImage))
            {
                return FiveMSessionPresence.Indeterminate;
            }

            SafePath.EnsureNoReparsePoints(normalizedImage);
            return FiveMSessionPresence.Present;
        }
        catch (Exception exception) when (IsPathInspectionFailure(exception))
        {
            return FiveMSessionPresence.Indeterminate;
        }
    }

    private static bool TryValidateLegacyRoot(string legacyRoot, out string normalizedRoot)
    {
        normalizedRoot = string.Empty;
        try
        {
            normalizedRoot = SafePath.Normalize(legacyRoot);
            var dataRoot = Path.Combine(normalizedRoot, "FiveM.app", "data");
            if (!Directory.Exists(normalizedRoot) || !Directory.Exists(dataRoot))
            {
                return false;
            }

            SafePath.EnsureNoReparsePoints(normalizedRoot);
            SafePath.EnsureNoReparsePoints(dataRoot);
            return true;
        }
        catch (Exception exception) when (IsPathInspectionFailure(exception))
        {
            normalizedRoot = string.Empty;
            return false;
        }
    }

    private static bool IsAllowedExecutableLeaf(string executableLeaf) =>
        executableLeaf.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
        && WindowsFiveMProcessInspector.LooksLikeFiveMProcessName(
            Path.GetFileNameWithoutExtension(executableLeaf));

    private static bool IsDescendant(string normalizedRoot, string normalizedCandidate) =>
        normalizedCandidate.StartsWith(
            normalizedRoot + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);

    private static bool IsProcessInspectionFailure(Exception exception) =>
        exception is InvalidOperationException
            or Win32Exception
            or NotSupportedException
            or UnauthorizedAccessException
            or SecurityException;

    private static bool IsPathInspectionFailure(Exception exception) =>
        exception is ArgumentException
            or IOException
            or NotSupportedException
            or UnauthorizedAccessException
            or SecurityException;
}
