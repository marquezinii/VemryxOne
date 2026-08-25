using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Vemryx.One.App.Services;

/// <summary>
/// Replaces only the known FiveMCleaner shortcuts that resolve to this exact
/// installation. Runtime updates intentionally preserve legacy executable and
/// data-path contracts, but they must not leave the old public identity behind.
/// </summary>
internal static class LegacyBrandMigration
{
    private const string ProductName = "Vemryx One";
    private const string LegacyProductName = "FiveMCleaner";
    private static readonly string[] LegacyUninstallShortcutNames =
    [
        "Desinstalar o FiveMCleaner.lnk",
        "Uninstall FiveMCleaner.lnk",
    ];

    internal static void TryMigrate(string installRoot, string appExecutablePath)
    {
        TryMigrate(
            installRoot,
            appExecutablePath,
            Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
    }

    internal static bool TryMigrate(
        string installRoot,
        string appExecutablePath,
        string programsRoot,
        string desktopRoot)
    {
        try
        {
            var launcherPath = Path.Combine(RequireDirectory(installRoot), "FiveMCleaner.Launcher.exe");
            var iconPath = Path.GetFullPath(appExecutablePath);
            if (!File.Exists(launcherPath) || !File.Exists(iconPath)) return false;

            if (!Path.IsPathFullyQualified(programsRoot)) return false;
            if (!Path.IsPathFullyQualified(desktopRoot)) return false;

            var oldGroup = Path.Combine(programsRoot, LegacyProductName);
            var newGroup = Path.Combine(programsRoot, ProductName);
            var oldMainShortcut = Path.Combine(oldGroup, $"{LegacyProductName}.lnk");
            if (TryReadShortcutTarget(oldMainShortcut, out var mainTarget)
                && PathsEqual(mainTarget, launcherPath))
            {
                if (!EnsureShortcut(
                    Path.Combine(newGroup, $"{ProductName}.lnk"),
                    launcherPath,
                    installRoot,
                    iconPath,
                    ProductName))
                {
                    return false;
                }

                File.Delete(oldMainShortcut);
            }

            var uninstallerPath = Path.Combine(installRoot, "unins000.exe");
            var oldUninstallShortcuts = LegacyUninstallShortcutNames
                .Select(name => Path.Combine(oldGroup, name))
                .Where(path => TryReadShortcutTarget(path, out var target) && PathsEqual(target, uninstallerPath))
                .ToArray();
            if (oldUninstallShortcuts.Length != 0
                && File.Exists(uninstallerPath)
                && EnsureShortcut(
                    Path.Combine(newGroup, $"Desinstalar {ProductName}.lnk"),
                    uninstallerPath,
                    installRoot,
                    uninstallerPath,
                    $"Desinstalar {ProductName}"))
            {
                foreach (var path in oldUninstallShortcuts)
                {
                    File.Delete(path);
                }
            }

            TryDeleteEmptyDirectory(oldGroup);

            var oldDesktopShortcut = Path.Combine(desktopRoot, $"{LegacyProductName}.lnk");
            if (TryReadShortcutTarget(oldDesktopShortcut, out var desktopTarget)
                && PathsEqual(desktopTarget, launcherPath)
                && EnsureShortcut(
                    Path.Combine(desktopRoot, $"{ProductName}.lnk"),
                    launcherPath,
                    installRoot,
                    iconPath,
                    ProductName))
            {
                File.Delete(oldDesktopShortcut);
            }

            return true;
        }
        catch (Exception exception) when (exception is not (
            OutOfMemoryException or StackOverflowException or AccessViolationException))
        {
            // Branding never blocks the application or its signed runtime update.
            return false;
        }
    }

    internal static bool PathsEqual(string? candidate, string expected) => candidate is not null
        && Path.GetFullPath(candidate).Equals(Path.GetFullPath(expected), StringComparison.OrdinalIgnoreCase);

    private static string RequireDirectory(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!Path.IsPathFullyQualified(value)) throw new ArgumentException("O diretório da instalação precisa ser absoluto.", nameof(value));
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(value));
    }

    private static bool EnsureShortcut(string shortcutPath, string targetPath, string workingDirectory, string iconPath, string description)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(shortcutPath)!);
        object? shell = null;
        object? shortcut = null;
        try
        {
            shell = CreateShell();
            shortcut = ((dynamic)shell).CreateShortcut(shortcutPath);
            ((dynamic)shortcut).TargetPath = targetPath;
            ((dynamic)shortcut).WorkingDirectory = workingDirectory;
            ((dynamic)shortcut).IconLocation = $"{iconPath},0";
            ((dynamic)shortcut).Description = description;
            ((dynamic)shortcut).Save();
        }
        finally
        {
            ReleaseComObject(shortcut);
            ReleaseComObject(shell);
        }

        return TryReadShortcutTarget(shortcutPath, out var actualTarget) && PathsEqual(actualTarget, targetPath);
    }

    private static bool TryReadShortcutTarget(string shortcutPath, out string? target)
    {
        target = null;
        if (!File.Exists(shortcutPath)) return false;

        object? shell = null;
        object? shortcut = null;
        try
        {
            shell = CreateShell();
            shortcut = ((dynamic)shell).CreateShortcut(shortcutPath);
            target = ((dynamic)shortcut).TargetPath as string;
            return !string.IsNullOrWhiteSpace(target);
        }
        finally
        {
            ReleaseComObject(shortcut);
            ReleaseComObject(shell);
        }
    }

    private static object CreateShell() => Activator.CreateInstance(
        Type.GetTypeFromProgID("WScript.Shell")
        ?? throw new COMException("O Windows não disponibilizou o serviço de atalhos."))
        ?? throw new COMException("O Windows não criou o serviço de atalhos.");

    private static void ReleaseComObject(object? value)
    {
        if (value is not null && Marshal.IsComObject(value)) Marshal.FinalReleaseComObject(value);
    }

    private static void TryDeleteEmptyDirectory(string path)
    {
        if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path);
        }
    }
}
