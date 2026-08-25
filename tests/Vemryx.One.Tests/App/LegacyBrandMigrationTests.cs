using System;
using System.Collections.Generic;
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

    [Theory]
    [InlineData("FiveMCleaner.lnk")]
    [InlineData("Vemryx One.lnk")]
    public void TryMigrate_MovesBothKnownLegacyMainShortcutNames(string legacyName)
    {
        var paths = CreatePaths();
        var shortcuts = new FakeShortcutStore();
        shortcuts.Add(Path.Combine(paths.OldGroup, legacyName), paths.Launcher);

        Assert.True(LegacyBrandMigration.TryMigrate(paths.Install, paths.Icon, paths.Programs, paths.Desktop, shortcuts));
        Assert.False(shortcuts.Exists(Path.Combine(paths.OldGroup, legacyName)));
        Assert.Equal(paths.Launcher, shortcuts.Target(Path.Combine(paths.NewGroup, "Vemryx One.lnk")));
    }

    [Fact]
    public void TryMigrate_PreservesAnExistingDestinationShortcutAndTheLegacySource()
    {
        var paths = CreatePaths();
        var shortcuts = new FakeShortcutStore();
        var source = Path.Combine(paths.OldGroup, "FiveMCleaner.lnk");
        var destination = Path.Combine(paths.NewGroup, "Vemryx One.lnk");
        var customTarget = Path.Combine(root, "custom.exe");
        shortcuts.Add(source, paths.Launcher);
        shortcuts.Add(destination, customTarget);

        Assert.True(LegacyBrandMigration.TryMigrate(paths.Install, paths.Icon, paths.Programs, paths.Desktop, shortcuts));
        Assert.Equal(customTarget, shortcuts.Target(destination));
        Assert.True(shortcuts.Exists(source));
    }

    [Fact]
    public void TryMigrate_PreservesAnUnrelatedLegacyGroupAndShortcut()
    {
        var paths = CreatePaths();
        var shortcuts = new FakeShortcutStore();
        var source = Path.Combine(paths.OldGroup, "FiveMCleaner.lnk");
        shortcuts.Add(source, Path.Combine(root, "other.exe"));

        Assert.True(LegacyBrandMigration.TryMigrate(paths.Install, paths.Icon, paths.Programs, paths.Desktop, shortcuts));
        Assert.True(shortcuts.Exists(source));
        Assert.True(Directory.Exists(paths.OldGroup));
        Assert.False(shortcuts.Exists(Path.Combine(paths.NewGroup, "Vemryx One.lnk")));
    }

    [Fact]
    public void PathsEqual_OnlyAcceptsTheExactCanonicalInstallTarget()
    {
        var expected = Path.Combine(Path.GetTempPath(), "Vemryx One", "FiveMCleaner.Launcher.exe");

        Assert.True(LegacyBrandMigration.PathsEqual(Path.Combine(Path.GetTempPath(), "Vemryx One", ".", "FiveMCleaner.Launcher.exe"), expected));
        Assert.False(LegacyBrandMigration.PathsEqual(Path.Combine(Path.GetTempPath(), "Outra instalacao", "FiveMCleaner.Launcher.exe"), expected));
    }

    private (string Install, string Programs, string Desktop, string OldGroup, string NewGroup, string Launcher, string Icon) CreatePaths()
    {
        var install = Path.Combine(root, "install");
        var programs = Path.Combine(root, "Programs");
        var desktop = Path.Combine(root, "Desktop");
        var oldGroup = Path.Combine(programs, "FiveMCleaner");
        Directory.CreateDirectory(install);
        Directory.CreateDirectory(oldGroup);
        Directory.CreateDirectory(desktop);
        var launcher = Path.Combine(install, "FiveMCleaner.Launcher.exe");
        var icon = Path.Combine(root, "FiveMCleaner.exe");
        File.WriteAllText(launcher, "launcher");
        File.WriteAllText(icon, "icon");
        return (install, programs, desktop, oldGroup, Path.Combine(programs, "Vemryx One"), launcher, icon);
    }

    private sealed class FakeShortcutStore : ILegacyShortcutStore
    {
        private readonly Dictionary<string, string> links = new(StringComparer.OrdinalIgnoreCase);

        public void Add(string path, string target) => links[path] = target;
        public bool Exists(string path) => links.ContainsKey(path);
        public string? Target(string path) => links.GetValueOrDefault(path);
        public bool TryReadTarget(string path, out string? target) => links.TryGetValue(path, out target);
        public bool TryCreate(string path, ShortcutDefinition definition) => links.TryAdd(path, definition.TargetPath);
        public bool TryDelete(string path) => links.Remove(path);
    }
}
