using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using Xunit;

namespace Vemryx.One.Tests.App;

public sealed class BrandAssetTests
{
    [Fact]
    public void OfficialIconIsExportedForTheApplication()
    {
        var root = TestHelpers.FindRepositoryRoot();
        var sourcePath = Path.Combine(root, "docs", "brand", "vemryx-one-icon-source.png");
        var appPngPath = Path.Combine(root, "src", "Vemryx.One.App", "Assets", "VemryxOne.png");
        var appIcoPath = Path.Combine(root, "src", "Vemryx.One.App", "Assets", "VemryxOne.ico");

        Assert.Equal(SHA256.HashData(File.ReadAllBytes(sourcePath)), SHA256.HashData(File.ReadAllBytes(appPngPath)));

        var png = BitmapDecoder.Create(new Uri(appPngPath), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames.Single();
        Assert.Equal((1024, 1024), (png.PixelWidth, png.PixelHeight));
        Assert.Equal(4, png.Format.Masks.Count);

        var iconSizes = BitmapDecoder.Create(new Uri(appIcoPath), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad)
            .Frames
            .Select(frame => frame.PixelWidth)
            .ToHashSet();
        Assert.True(new[] { 16, 20, 24, 32, 40, 48, 64, 128, 256 }.All(iconSizes.Contains));

        Assert.False(File.Exists(Path.Combine(root, "src", "Vemryx.One.App", "Assets", "FiveMCleaner.png")));
        Assert.False(File.Exists(Path.Combine(root, "src", "Vemryx.One.App", "Assets", "FiveMCleaner.ico")));
    }
}
