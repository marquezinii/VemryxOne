using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.Windows;

public sealed class FiveMSessionProbeTests
{
    [Fact]
    public void ClassifyCandidate_ConfirmsAllowedImageInsideLegacyRoot()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var root = CreateLegacyRoot(temporaryDirectory);
        var image = CreateImage(root, "FiveM_b3258_GTAProcess.exe");

        var result = WindowsFiveMSessionProbe.ClassifyCandidate(
            "FiveM_b3258_GTAProcess",
            image,
            root);

        Assert.Equal(FiveMSessionPresence.Present, result);
    }

    [Fact]
    public void ClassifyCandidate_DoesNotTreatKnownNameWithoutImageAsPresent()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var root = CreateLegacyRoot(temporaryDirectory);

        var result = WindowsFiveMSessionProbe.ClassifyCandidate("FiveM", null, root);

        Assert.Equal(FiveMSessionPresence.Indeterminate, result);
    }

    [Fact]
    public void ClassifyCandidate_RequiresExistingLegacyDataRoot()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var root = temporaryDirectory.Combine("FiveM");
        Directory.CreateDirectory(root);
        var image = Path.Combine(root, "FiveM.exe");
        File.WriteAllText(image, string.Empty);

        var result = WindowsFiveMSessionProbe.ClassifyCandidate("FiveM", image, root);

        Assert.Equal(FiveMSessionPresence.Indeterminate, result);
    }

    [Fact]
    public void ClassifyCandidate_RejectsUnknownExecutableLeaf()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var root = CreateLegacyRoot(temporaryDirectory);
        var image = CreateImage(root, "unrelated.exe");

        var result = WindowsFiveMSessionProbe.ClassifyCandidate("FiveM", image, root);

        Assert.Equal(FiveMSessionPresence.AbsentConfirmed, result);
    }

    [Fact]
    public void ClassifyCandidate_RejectsSiblingThatSharesRootPrefix()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var root = CreateLegacyRoot(temporaryDirectory);
        var sibling = temporaryDirectory.Combine("FiveM-sibling");
        Directory.CreateDirectory(sibling);
        var image = Path.Combine(sibling, "FiveM.exe");
        File.WriteAllText(image, string.Empty);

        var result = WindowsFiveMSessionProbe.ClassifyCandidate("FiveM", image, root);

        Assert.Equal(FiveMSessionPresence.AbsentConfirmed, result);
    }

    [Fact]
    public void ClassifyCandidate_DoesNotTreatStandaloneGtaAsFiveMSession()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var root = CreateLegacyRoot(temporaryDirectory);
        var image = CreateImage(root, "GTA5.exe");

        var result = WindowsFiveMSessionProbe.ClassifyCandidate("GTA5", image, root);

        Assert.Equal(FiveMSessionPresence.AbsentConfirmed, result);
    }

    [Fact]
    public void ClassifyCandidate_RejectsImageThroughReparsePointWhenSupported()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var root = CreateLegacyRoot(temporaryDirectory);
        var outside = temporaryDirectory.Combine("outside");
        Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(outside, "FiveM.exe"), string.Empty);
        var link = Path.Combine(root, "FiveM.app", "linked-runtime");
        try
        {
            Directory.CreateSymbolicLink(link, outside);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
            or IOException
            or PlatformNotSupportedException)
        {
            return;
        }

        var result = WindowsFiveMSessionProbe.ClassifyCandidate(
            "FiveM",
            Path.Combine(link, "FiveM.exe"),
            root);

        Assert.Equal(FiveMSessionPresence.Indeterminate, result);
    }

    [Fact]
    public void ClassifyCandidate_RejectsLegacyRootReparsePointWhenSupported()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var realRoot = temporaryDirectory.Combine("real-FiveM");
        Directory.CreateDirectory(Path.Combine(realRoot, "FiveM.app", "data"));
        CreateImage(realRoot, "FiveM.exe");
        var linkedRoot = temporaryDirectory.Combine("linked-FiveM");
        try
        {
            Directory.CreateSymbolicLink(linkedRoot, realRoot);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
            or IOException
            or PlatformNotSupportedException)
        {
            return;
        }

        var result = WindowsFiveMSessionProbe.ClassifyCandidate(
            "FiveM",
            Path.Combine(linkedRoot, "FiveM.app", "FiveM.exe"),
            linkedRoot);

        Assert.Equal(FiveMSessionPresence.Indeterminate, result);
    }

    private static string CreateLegacyRoot(TemporaryDirectory temporaryDirectory)
    {
        var root = temporaryDirectory.Combine("FiveM");
        Directory.CreateDirectory(Path.Combine(root, "FiveM.app", "data"));
        return root;
    }

    private static string CreateImage(string root, string leaf)
    {
        var image = Path.Combine(root, "FiveM.app", leaf);
        File.WriteAllText(image, string.Empty);
        return image;
    }
}
