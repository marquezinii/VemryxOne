using System.IO.Compression;
using System.Security.Cryptography;
using Ralven.UpdateRuntime;
using Xunit;

namespace Ralven.Tests.UpdateRuntime;

public sealed class RuntimePackageStagerTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "RalvenStager", Guid.NewGuid().ToString("N"));
    public void Dispose() { if (Directory.Exists(root)) Directory.Delete(root, true); }

    [Fact]
    public void Stage_VerifiesArchiveAndEveryExtractedFile()
    {
        Directory.CreateDirectory(root);
        var source = Path.Combine(root, "source");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "Ralven.exe"), "payload");
        var fileHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(source, "Ralven.exe")))).ToLowerInvariant();
        File.WriteAllText(Path.Combine(source, "SHA256SUMS.txt"), $"{fileHash}  Ralven.exe");
        var zip = Path.Combine(root, "package.zip");
        ZipFile.CreateFromDirectory(source, zip);
        var zipHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(zip)));

        var result = new RuntimePackageStager(Path.Combine(root, "runtime")).Stage(
            zip, "2.0.0", zipHash, new FileInfo(zip).Length, TestContext.Current.CancellationToken);

        Assert.True(File.Exists(Path.Combine(result, "Ralven.exe")));
    }

    [Fact]
    public void Stage_RejectsAnExtractedFileMissingFromTheFileManifest()
    {
        Directory.CreateDirectory(root);
        var source = Path.Combine(root, "source");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "Ralven.exe"), "payload");
        File.WriteAllText(Path.Combine(source, "undeclared.dll"), "surprise");
        var fileHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(source, "Ralven.exe"))));
        File.WriteAllText(Path.Combine(source, "SHA256SUMS.txt"), $"{fileHash}  Ralven.exe");
        var zip = Path.Combine(root, "package.zip");
        ZipFile.CreateFromDirectory(source, zip);
        var zipHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(zip)));

        Assert.Throws<InvalidDataException>(() =>
            new RuntimePackageStager(Path.Combine(root, "runtime")).Stage(
                zip, "2.0.0", zipHash, new FileInfo(zip).Length, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Stage_ReExtractsWhenExistingDirectoryIsCorrupt()
    {
        var runtimeRoot = Path.Combine(root, "runtime");
        var stager = new RuntimePackageStager(runtimeRoot);

        Directory.CreateDirectory(root);
        var source = Path.Combine(root, "source");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "Ralven.exe"), "payload");
        var fileHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(source, "Ralven.exe")))).ToLowerInvariant();
        File.WriteAllText(Path.Combine(source, "SHA256SUMS.txt"), $"{fileHash}  Ralven.exe");
        var zip = Path.Combine(root, "package.zip");
        ZipFile.CreateFromDirectory(source, zip);
        var zipHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(zip)));

        var result = stager.Stage(zip, "2.0.0", zipHash, new FileInfo(zip).Length, TestContext.Current.CancellationToken);
        Assert.True(File.Exists(Path.Combine(result, "Ralven.exe")));

        // Corrupt the existing version directory by removing the manifest
        File.Delete(Path.Combine(result, "SHA256SUMS.txt"));

        // Re-staging should re-extract and succeed
        var result2 = stager.Stage(zip, "2.0.0", zipHash, new FileInfo(zip).Length, TestContext.Current.CancellationToken);
        Assert.True(File.Exists(Path.Combine(result2, "Ralven.exe")));
        Assert.True(File.Exists(Path.Combine(result2, "SHA256SUMS.txt")));
    }

    [Fact]
    public void Stage_ReExtractsWhenExistingDirectoryManifestIsTampered()
    {
        var runtimeRoot = Path.Combine(root, "runtime");
        var stager = new RuntimePackageStager(runtimeRoot);

        Directory.CreateDirectory(root);
        var source = Path.Combine(root, "source");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "Ralven.exe"), "payload");
        var fileHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(source, "Ralven.exe")))).ToLowerInvariant();
        File.WriteAllText(Path.Combine(source, "SHA256SUMS.txt"), $"{fileHash}  Ralven.exe");
        var zip = Path.Combine(root, "package.zip");
        ZipFile.CreateFromDirectory(source, zip);
        var zipHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(zip)));

        var result = stager.Stage(zip, "2.0.0", zipHash, new FileInfo(zip).Length, TestContext.Current.CancellationToken);
        Assert.True(File.Exists(Path.Combine(result, "Ralven.exe")));

        // Tamper with the manifest (wrong hash)
        var tamperedHash = Convert.ToHexString(SHA256.HashData("tampered"u8.ToArray())).ToLowerInvariant();
        File.WriteAllText(Path.Combine(result, "SHA256SUMS.txt"), $"{tamperedHash}  Ralven.exe");

        // Re-staging should re-extract and succeed
        var result2 = stager.Stage(zip, "2.0.0", zipHash, new FileInfo(zip).Length, TestContext.Current.CancellationToken);
        Assert.True(File.Exists(Path.Combine(result2, "Ralven.exe")));
        Assert.True(File.Exists(Path.Combine(result2, "SHA256SUMS.txt")));
    }
}
