using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Vemryx.One.App.Services;

internal sealed record ShortcutDefinition(string TargetPath, string WorkingDirectory, string IconPath, string Description);

internal interface ILegacyShortcutStore
{
    bool Exists(string path);
    bool TryReadTarget(string path, out string? target);
    bool TryCreate(string path, ShortcutDefinition definition);
    bool TryDelete(string path);
}

/// <summary>
/// Replaces only verified FiveMCleaner shortcuts that resolve to this exact
/// installation. The runtime and data-path contracts remain intentionally legacy.
/// </summary>
internal static class LegacyBrandMigration
{
    private const string ProductName = "Vemryx One";
    private const string LegacyProductName = "FiveMCleaner";
    private static readonly string[] LegacyMainShortcutNames =
    ["FiveMCleaner.lnk", "Vemryx One.lnk"];
    private static readonly string[] LegacyUninstallShortcutNames =
    ["Desinstalar o FiveMCleaner.lnk", "Uninstall FiveMCleaner.lnk"];

    internal static void TryMigrate(string installRoot, string appExecutablePath) =>
        TryMigrate(
            installRoot,
            appExecutablePath,
            Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            new WindowsShortcutStore());

    internal static bool TryMigrate(
        string installRoot,
        string appExecutablePath,
        string programsRoot,
        string desktopRoot,
        ILegacyShortcutStore shortcuts)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(shortcuts);
            installRoot = RequireDirectory(installRoot);
            var launcherPath = Path.Combine(installRoot, "FiveMCleaner.Launcher.exe");
            var iconPath = Path.GetFullPath(appExecutablePath);
            if (!File.Exists(launcherPath) || !File.Exists(iconPath)
                || !Path.IsPathFullyQualified(programsRoot) || !Path.IsPathFullyQualified(desktopRoot)) return false;

            var oldGroup = Path.Combine(programsRoot, LegacyProductName);
            var newGroup = Path.Combine(programsRoot, ProductName);
            var removedFromOldGroup = false;
            foreach (var name in LegacyMainShortcutNames)
            {
                removedFromOldGroup |= TryMigrateShortcut(
                    shortcuts,
                    Path.Combine(oldGroup, name),
                    Path.Combine(newGroup, $"{ProductName}.lnk"),
                    new(launcherPath, installRoot, iconPath, ProductName));
            }

            var uninstallerPath = Path.Combine(installRoot, "unins000.exe");
            if (File.Exists(uninstallerPath))
            {
                foreach (var name in LegacyUninstallShortcutNames)
                {
                    removedFromOldGroup |= TryMigrateShortcut(
                        shortcuts,
                        Path.Combine(oldGroup, name),
                        Path.Combine(newGroup, $"Desinstalar {ProductName}.lnk"),
                        new(uninstallerPath, installRoot, uninstallerPath, $"Desinstalar {ProductName}"));
                }
            }

            if (removedFromOldGroup) TryDeleteEmptyDirectory(oldGroup);

            TryMigrateShortcut(
                shortcuts,
                Path.Combine(desktopRoot, $"{LegacyProductName}.lnk"),
                Path.Combine(desktopRoot, $"{ProductName}.lnk"),
                new(launcherPath, installRoot, iconPath, ProductName));
            return true;
        }
        catch (Exception exception) when (exception is not (
            OutOfMemoryException or StackOverflowException or AccessViolationException))
        {
            // Branding is best-effort and must never delay or block startup.
            return false;
        }
    }

    internal static bool PathsEqual(string? candidate, string expected) => candidate is not null
        && Path.GetFullPath(candidate).Equals(Path.GetFullPath(expected), StringComparison.OrdinalIgnoreCase);

    private static bool TryMigrateShortcut(
        ILegacyShortcutStore shortcuts,
        string sourcePath,
        string destinationPath,
        ShortcutDefinition definition)
    {
        if (!shortcuts.TryReadTarget(sourcePath, out var target) || !PathsEqual(target, definition.TargetPath)) return false;
        if (!EnsureShortcut(shortcuts, destinationPath, definition)) return false;
        return shortcuts.TryDelete(sourcePath);
    }

    private static bool EnsureShortcut(ILegacyShortcutStore shortcuts, string path, ShortcutDefinition definition)
    {
        if (shortcuts.Exists(path))
        {
            return shortcuts.TryReadTarget(path, out var target) && PathsEqual(target, definition.TargetPath);
        }

        return shortcuts.TryCreate(path, definition)
            && shortcuts.TryReadTarget(path, out var createdTarget)
            && PathsEqual(createdTarget, definition.TargetPath);
    }

    private static string RequireDirectory(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!Path.IsPathFullyQualified(value)) throw new ArgumentException("O diretório da instalação precisa ser absoluto.", nameof(value));
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(value));
    }

    private static void TryDeleteEmptyDirectory(string path)
    {
        if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any()) Directory.Delete(path);
    }

    private sealed class WindowsShortcutStore : ILegacyShortcutStore
    {
        public bool Exists(string path) => File.Exists(path);

        public bool TryReadTarget(string path, out string? target)
        {
            target = null;
            if (!File.Exists(path)) return false;
            object? shell = null;
            object? shortcut = null;
            try
            {
                shell = CreateShell();
                shortcut = ((dynamic)shell).CreateShortcut(path);
                target = ((dynamic)shortcut).TargetPath as string;
                return !string.IsNullOrWhiteSpace(target);
            }
            finally
            {
                Release(shortcut);
                Release(shell);
            }
        }

        public bool TryCreate(string path, ShortcutDefinition definition)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            object? shell = null;
            object? shortcut = null;
            try
            {
                shell = CreateShell();
                shortcut = ((dynamic)shell).CreateShortcut(path);
                ((dynamic)shortcut).TargetPath = definition.TargetPath;
                ((dynamic)shortcut).WorkingDirectory = definition.WorkingDirectory;
                ((dynamic)shortcut).IconLocation = $"{definition.IconPath},0";
                ((dynamic)shortcut).Description = definition.Description;
                ((dynamic)shortcut).Save();
                return true;
            }
            finally
            {
                Release(shortcut);
                Release(shell);
            }
        }

        public bool TryDelete(string path)
        {
            File.Delete(path);
            return !File.Exists(path);
        }

        private static object CreateShell() => Activator.CreateInstance(
            Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new COMException("O Windows não disponibilizou o serviço de atalhos."))
            ?? throw new COMException("O Windows não criou o serviço de atalhos.");

        private static void Release(object? value)
        {
            if (value is not null && Marshal.IsComObject(value)) Marshal.FinalReleaseComObject(value);
        }
    }
}
