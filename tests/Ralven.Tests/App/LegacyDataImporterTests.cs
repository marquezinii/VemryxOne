using Ralven.App.Services;
using Xunit;

namespace Ralven.Tests.App;

public sealed class LegacyDataImporterTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "RalvenDataImport", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    [Fact]
    public void TryImport_CopiesOnlyUserStateOnceWithoutOverwritingOrDeletingTheSource()
    {
        var source = Path.Combine(root, "legacy");
        var destination = Path.Combine(root, "Ralven");
        Directory.CreateDirectory(Path.Combine(source, "avatars"));
        Directory.CreateDirectory(Path.Combine(source, "Updates"));
        Directory.CreateDirectory(destination);
        File.WriteAllText(Path.Combine(source, "settings.json"), "legacy");
        File.WriteAllText(Path.Combine(source, "history.json"), "history");
        File.WriteAllText(Path.Combine(source, "avatars", "user.png"), "avatar");
        File.WriteAllText(Path.Combine(source, "Updates", "unsupported.exe"), "binary");
        File.WriteAllText(Path.Combine(destination, "settings.json"), "current");

        Assert.True(LegacyDataImporter.TryImport([source], destination));
        Assert.Equal("current", File.ReadAllText(Path.Combine(destination, "settings.json")));
        Assert.Equal("history", File.ReadAllText(Path.Combine(destination, "history.json")));
        Assert.Equal("avatar", File.ReadAllText(Path.Combine(destination, "avatars", "user.png")));
        Assert.False(Directory.Exists(Path.Combine(destination, "Updates")));
        Assert.True(File.Exists(Path.Combine(source, "history.json")));

        File.WriteAllText(Path.Combine(source, "firebase.session"), "late");
        Assert.False(LegacyDataImporter.TryImport([source], destination));
        Assert.False(File.Exists(Path.Combine(destination, "firebase.session")));
    }

    [Fact]
    public void TryImport_RefusesADestinationReparsePoint()
    {
        var source = Path.Combine(root, "legacy");
        var destination = Path.Combine(root, "Ralven");
        var outside = Path.Combine(root, "outside");
        Directory.CreateDirectory(Path.Combine(source, "avatars"));
        Directory.CreateDirectory(destination);
        Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(source, "avatars", "user.png"), "avatar");

        try
        {
            Directory.CreateSymbolicLink(Path.Combine(destination, "avatars"), outside);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
            or IOException
            or PlatformNotSupportedException)
        {
            return;
        }

        Assert.False(LegacyDataImporter.TryImport([source], destination));
        Assert.False(File.Exists(Path.Combine(outside, "user.png")));
    }
}
