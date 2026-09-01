using System.IO.Compression;
using System.Security.Cryptography;
using Ralven.UpdateRuntime;
using Xunit;

namespace Ralven.Tests.UpdateRuntime;

public sealed class UpdateLifecycleIntegrationTests : IDisposable
{
    private readonly string root = Path.Combine(
        Path.GetTempPath(), "RalvenUpdateLifecycle", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    [Fact]
    public void StagedUpdate_CommitsOnHealthAndRollsBackTheNextUnhealthyCandidate()
    {
        var runtimeRoot = Path.Combine(root, "Runtime");
        var activation = new RuntimeActivationStore(runtimeRoot);
        Directory.CreateDirectory(Path.Combine(activation.VersionsRoot, "1.0.0"));
        activation.Activate("1.0.0");

        StageActivateAndLaunch(runtimeRoot, "1.0.0", "2.0.0", confirmHealth: true);
        Assert.Equal(RecoveryDecision.Healthy, new RecoveryCoordinator(runtimeRoot).Reconcile(
            DateTimeOffset.UtcNow, TimeSpan.FromSeconds(45)));
        Assert.Equal("2.0.0", activation.ReadActiveVersion());
        Assert.False(new UpdateRecoveryJournal(runtimeRoot).TryRead(out _));

        var unhealthy = StageActivateAndLaunch(runtimeRoot, "2.0.0", "3.0.0", confirmHealth: false);
        Assert.Equal(RecoveryDecision.RolledBack, new RecoveryCoordinator(runtimeRoot).Reconcile(
            unhealthy.CandidateLaunchedAtUtc!.Value.AddSeconds(46), TimeSpan.FromSeconds(45)));
        Assert.Equal("2.0.0", activation.ReadActiveVersion());
        Assert.False(new UpdateRecoveryJournal(runtimeRoot).TryRead(out _));
    }

    private UpdateTransaction StageActivateAndLaunch(
        string runtimeRoot,
        string previousVersion,
        string candidateVersion,
        bool confirmHealth)
    {
        var package = CreatePackage(candidateVersion);
        new RuntimePackageStager(runtimeRoot).Stage(
            package,
            candidateVersion,
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(package))),
            new FileInfo(package).Length,
            TestContext.Current.CancellationToken);

        var journal = new UpdateRecoveryJournal(runtimeRoot);
        var transaction = journal.Begin(previousVersion, candidateVersion);
        new RuntimeActivationStore(runtimeRoot).Activate(candidateVersion);
        transaction = journal.MarkCandidateLaunched(transaction);
        if (confirmHealth) new UpdateHealthReceiptStore(runtimeRoot).Confirm(transaction);
        return transaction;
    }

    private string CreatePackage(string version)
    {
        var source = Path.Combine(root, $"source-{version}");
        Directory.CreateDirectory(source);
        var executable = Path.Combine(source, "Ralven.exe");
        File.WriteAllText(executable, $"runtime {version}");
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(executable))).ToLowerInvariant();
        File.WriteAllText(Path.Combine(source, "SHA256SUMS.txt"), $"{hash}  Ralven.exe");
        var package = Path.Combine(root, $"runtime-{version}.zip");
        ZipFile.CreateFromDirectory(source, package);
        return package;
    }
}
