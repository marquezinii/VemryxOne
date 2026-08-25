using System;
using System.IO;
using Vemryx.One.App.Services;
using Xunit;

namespace Vemryx.One.Tests.App;

public sealed class LegacyBrandMigrationTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "VemryxOneBrandMigration", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    [Fact]
    public void PathsEqual_OnlyAcceptsTheExactCanonicalInstallTarget()
    {
        var expected = Path.Combine(Path.GetTempPath(), "Vemryx One", "FiveMCleaner.Launcher.exe");

        Assert.True(LegacyBrandMigration.PathsEqual(
            Path.Combine(Path.GetTempPath(), "Vemryx One", ".", "FiveMCleaner.Launcher.exe"), expected));
        Assert.False(LegacyBrandMigration.PathsEqual(
            Path.Combine(Path.GetTempPath(), "Outra instalacao", "FiveMCleaner.Launcher.exe"), expected));
        Assert.False(LegacyBrandMigration.PathsEqual(null, expected));
    }

    [Fact]
    public void TryMigrate_CreatesNewLinksBeforeRemovingOnlyMatchingLegacyLinks()
    {
        var install = Path.Combine(root, "install");
        var programs = Path.Combine(root, "Programs");
        var desktop = Path.Combine(root, "Desktop");
        var launcher = Path.Combine(install, "FiveMCleaner.Launcher.exe");
        var icon = Path.Combine(root, "FiveMCleaner.exe");
        Directory.CreateDirectory(install);
        Directory.CreateDirectory(Path.Combine(programs, "FiveMCleaner"));
        Directory.CreateDirectory(desktop);
        File.WriteAllText(launcher, "launcher");
        File.WriteAllText(icon, "icon");
        CreateShortcut(Path.Combine(programs, "FiveMCleaner", "FiveMCleaner.lnk"), launcher);
        CreateShortcut(Path.Combine(desktop, "FiveMCleaner.lnk"), launcher);

        var migrated = LegacyBrandMigration.TryMigrate(install, icon, programs, desktop);

        Assert.True(migrated);
        Assert.True(File.Exists(Path.Combine(programs, "Vemryx One", "Vemryx One.lnk")));
        Assert.True(File.Exists(Path.Combine(desktop, "Vemryx One.lnk")));
        Assert.False(File.Exists(Path.Combine(programs, "FiveMCleaner", "FiveMCleaner.lnk")));
        Assert.False(File.Exists(Path.Combine(desktop, "FiveMCleaner.lnk")));
    }

    [Fact]
    public void TryMigrate_PreservesALegacyShortcutThatDoesNotPointToThisInstallation()
    {
        var install = Path.Combine(root, "install");
        var programs = Path.Combine(root, "Programs");
        var desktop = Path.Combine(root, "Desktop");
        var launcher = Path.Combine(install, "FiveMCleaner.Launcher.exe");
        var icon = Path.Combine(root, "FiveMCleaner.exe");
        var otherTarget = Path.Combine(root, "other.exe");
        Directory.CreateDirectory(install);
        Directory.CreateDirectory(Path.Combine(programs, "FiveMCleaner"));
        Directory.CreateDirectory(desktop);
        File.WriteAllText(launcher, "launcher");
        File.WriteAllText(icon, "icon");
        File.WriteAllText(otherTarget, "other");
        var legacyShortcut = Path.Combine(programs, "FiveMCleaner", "FiveMCleaner.lnk");
        CreateShortcut(legacyShortcut, otherTarget);

        Assert.True(LegacyBrandMigration.TryMigrate(install, icon, programs, desktop));
        Assert.True(File.Exists(legacyShortcut));
        Assert.False(File.Exists(Path.Combine(programs, "Vemryx One", "Vemryx One.lnk")));
    }

    private static void CreateShortcut(string path, string target)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Windows Script Host indisponível para o teste de atalhos.");
        dynamic shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("Windows não criou o serviço de atalhos para o teste.");
        dynamic shortcut = shell.CreateShortcut(path);
        shortcut.TargetPath = target;
        shortcut.Save();
    }
}
