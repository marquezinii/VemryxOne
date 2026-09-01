using System.Diagnostics;

namespace Ralven.Windows.Infrastructure;

public interface IFiveMProcessInspector
{
    bool IsAnyRunning();

    bool IsRunningFrom(string installationRoot);
}

public sealed class WindowsFiveMProcessInspector : IFiveMProcessInspector
{
    public bool IsAnyRunning() => IsRunningCore(normalizedInstallationRoot: null);

    public bool IsRunningFrom(string installationRoot)
    {
        return IsRunningCore(SafePath.Normalize(installationRoot));
    }

    internal static bool LooksLikeFiveMExecutablePath(
        string processName,
        string executablePath)
    {
        return IsVerifiedFiveMExecutablePath(
            processName,
            executablePath,
            normalizedInstallationRoot: null);
    }

    internal static bool IsVerifiedFiveMExecutablePath(
        string processName,
        string executablePath,
        string? normalizedInstallationRoot)
    {
        if (!LooksLikeFiveMProcessName(processName)
            || string.IsNullOrWhiteSpace(executablePath))
        {
            return false;
        }

        var fullPath = Path.GetFullPath(executablePath);
        var imageName = Path.GetFileNameWithoutExtension(fullPath);
        if (!imageName.Equals(processName, StringComparison.OrdinalIgnoreCase)
            || !LooksLikeFiveMProcessName(imageName))
        {
            return false;
        }

        if (normalizedInstallationRoot is not null
            && IsPathWithinRoot(fullPath, normalizedInstallationRoot))
        {
            return true;
        }

        var directory = Path.GetDirectoryName(fullPath);
        while (!string.IsNullOrWhiteSpace(directory))
        {
            if (Path.GetFileName(directory).Equals("FiveM.app", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            directory = Path.GetDirectoryName(directory);
        }

        var imageDirectory = Path.GetDirectoryName(fullPath);
        return imageDirectory is not null
            && (imageName.Equals("FiveM", StringComparison.OrdinalIgnoreCase)
                || imageName.Equals("CitizenFX", StringComparison.OrdinalIgnoreCase))
            && Directory.Exists(Path.Combine(imageDirectory, "FiveM.app"));
    }

    public static bool LooksLikeFiveMProcessName(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return false;
        }

        // Process.ProcessName does not include the .exe suffix. FiveM's runtime
        // children use the FiveM_* and CitizenFX_* families. Matching a bare
        // substring would make Ralven detect itself as the game.
        return processName.Equals("FiveM", StringComparison.OrdinalIgnoreCase)
            || processName.StartsWith("FiveM_", StringComparison.OrdinalIgnoreCase)
            || processName.Equals("CitizenFX", StringComparison.OrdinalIgnoreCase)
            || processName.StartsWith("CitizenFX_", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRunningCore(string? normalizedInstallationRoot)
    {
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                var processName = GetProcessNameOrEmpty(process);
                if (!LooksLikeFiveMProcessName(processName))
                {
                    continue;
                }

                var executablePath = TryGetExecutablePath(process);
                if (executablePath is null)
                {
                    // O nome continua disponível quando MainModule é negado por uma
                    // diferença de elevação. Nesse caso, bloquear é a decisão segura.
                    return true;
                }

                if (IsVerifiedFiveMExecutablePath(
                        processName,
                        executablePath,
                        normalizedInstallationRoot))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsPathWithinRoot(string fullPath, string normalizedRoot)
    {
        return fullPath.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase)
            || fullPath.StartsWith(
                normalizedRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);
    }

    private static string GetProcessNameOrEmpty(Process process)
    {
        try
        {
            return process.ProcessName;
        }
        catch (Exception exception) when (exception is InvalidOperationException
            or System.ComponentModel.Win32Exception
            or NotSupportedException)
        {
            return string.Empty;
        }
    }

    private static string? TryGetExecutablePath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception
            or NotSupportedException)
        {
            return null;
        }
    }
}
