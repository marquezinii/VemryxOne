using Ralven.Contracts;
using Ralven.Windows.Actions;
using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.Windows;

public sealed class StaleAuthDataRepairActionTests
{
    [Fact]
    public async Task Apply_DoesNotTreatUnrelatedErrorAsEntitlementFailure()
    {
        using var fixture = new Fixture("Social Club initialized\ntexture streaming error");

        var result = await fixture.Action.ApplyAsync(fixture.Context, CancellationToken.None);

        Assert.False(result.Changed);
        Assert.Equal(ActionExecutionOutcome.Verified, result.Outcome);
        Assert.True(File.Exists(fixture.RosIdPath));
        Assert.True(Directory.Exists(fixture.DigitalEntitlementsRoot));
    }

    [Fact]
    public async Task Apply_SkipsWhenDiagnosticLogIsUnavailable()
    {
        using var fixture = new Fixture(log: null);

        var result = await fixture.Action.ApplyAsync(fixture.Context, CancellationToken.None);

        Assert.False(result.Changed);
        Assert.Equal(ActionExecutionOutcome.Skipped, result.Outcome);
    }

    [Fact]
    public async Task Apply_SkipsWhenDiagnosticLogIsOlderThanTheCurrentSessionWindow()
    {
        using var fixture = new Fixture("entitlement error");
        File.SetLastWriteTimeUtc(fixture.LogPath, DateTime.UtcNow.AddDays(-2));

        var result = await fixture.Action.ApplyAsync(fixture.Context, CancellationToken.None);

        Assert.False(result.Changed);
        Assert.Equal(ActionExecutionOutcome.Skipped, result.Outcome);
        Assert.True(File.Exists(fixture.RosIdPath));
    }

    [Theory]
    [InlineData("Social Club authentication failed cases were fixed")]
    [InlineData("documentation mentions entitlement error")]
    [InlineData("not an entitlement error")]
    public async Task Apply_DoesNotMatchFailurePhraseInsideBenignText(string log)
    {
        using var fixture = new Fixture(log);

        var result = await fixture.Action.ApplyAsync(fixture.Context, CancellationToken.None);

        Assert.False(result.Changed);
        Assert.True(File.Exists(fixture.RosIdPath));
    }

    [Fact]
    public async Task Apply_SkipsWhenFailureIsDetectedButRepairTargetsAreMissing()
    {
        using var fixture = new Fixture("entitlement error", createItems: false);

        var result = await fixture.Action.ApplyAsync(fixture.Context, CancellationToken.None);

        Assert.False(result.Changed);
        Assert.Equal(ActionExecutionOutcome.Skipped, result.Outcome);
    }

    [Fact]
    public async Task ApplyAndRollback_RestoresAllowlistedEntitlementData()
    {
        using var fixture = new Fixture("[  12345] [error] Social Club authentication failed");

        var result = await fixture.Action.ApplyAsync(fixture.Context, CancellationToken.None);

        Assert.True(result.Changed);
        Assert.False(File.Exists(fixture.RosIdPath));
        Assert.False(Directory.Exists(fixture.DigitalEntitlementsRoot));

        await fixture.Action.RollbackAsync(
            fixture.Context,
            result.SnapshotJson,
            CancellationToken.None);

        Assert.Equal("ros", File.ReadAllText(fixture.RosIdPath));
        Assert.Equal(
            "token",
            File.ReadAllText(Path.Combine(fixture.DigitalEntitlementsRoot, "token.dat")));
    }

    [Fact]
    public async Task Rollback_PreservesBackupAndFailsWhenDestinationWasRecreated()
    {
        using var fixture = new Fixture("entitlement error");
        var result = await fixture.Action.ApplyAsync(fixture.Context, CancellationToken.None);
        File.WriteAllText(fixture.RosIdPath, "newer");

        await Assert.ThrowsAsync<IOException>(() => fixture.Action.RollbackAsync(
            fixture.Context,
            result.SnapshotJson,
            CancellationToken.None));

        Assert.Equal("newer", File.ReadAllText(fixture.RosIdPath));
        Assert.Equal(
            "ros",
            File.ReadAllText(Path.Combine(
                fixture.QuarantineRoot,
                fixture.Context.TransactionId.ToString("N"),
                "ros_id.dat")));
    }

    [Fact]
    public async Task Commit_RejectsSnapshotOutsideAllowlistBeforeDeletingAnything()
    {
        using var fixture = new Fixture("entitlement error");
        var outside = fixture.TemporaryDirectory.Combine("outside");
        Directory.CreateDirectory(outside);
        var outsideFile = Path.Combine(outside, "keep.txt");
        File.WriteAllText(outsideFile, "keep");
        var maliciousSnapshot = WindowsActionSnapshot.Serialize(new AuthDataRepairSnapshot(
        [
            new QuarantinedAuthItem(outside, outside, IsDirectory: true, Sha256: null, Entries: [])
        ]));

        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Action.CommitAsync(
            fixture.Context,
            maliciousSnapshot,
            CancellationToken.None));

        Assert.Equal("keep", File.ReadAllText(outsideFile));
    }

    [Fact]
    public void Constructor_RejectsSiblingTargetsWithAllowlistedBaseNames()
    {
        using var fixture = new Fixture("entitlement error");
        var siblingRosId = fixture.TemporaryDirectory.Combine("Sibling", "ros_id.dat");

        Assert.Throws<ArgumentException>(() => new StaleAuthDataRepairAction(
            fixture.FiveMAppRoot,
            fixture.InstallationRoot,
            siblingRosId,
            Path.GetDirectoryName(fixture.RosIdPath)!,
            fixture.DigitalEntitlementsRoot,
            Path.GetDirectoryName(fixture.DigitalEntitlementsRoot)!,
            fixture.QuarantineRoot,
            new FakeProcessInspector()));

        var siblingEntitlements = fixture.TemporaryDirectory.Combine("Sibling", "DigitalEntitlements");
        Assert.Throws<ArgumentException>(() => new StaleAuthDataRepairAction(
            fixture.FiveMAppRoot,
            fixture.InstallationRoot,
            fixture.RosIdPath,
            Path.GetDirectoryName(fixture.RosIdPath)!,
            siblingEntitlements,
            Path.GetDirectoryName(fixture.DigitalEntitlementsRoot)!,
            fixture.QuarantineRoot,
            new FakeProcessInspector()));
    }

    [Fact]
    public async Task Commit_RejectsContentInjectedIntoQuarantinedDirectory()
    {
        using var fixture = new Fixture("entitlement error");
        var result = await fixture.Action.ApplyAsync(fixture.Context, CancellationToken.None);
        var quarantinedEntitlements = Path.Combine(
            fixture.QuarantineRoot,
            fixture.Context.TransactionId.ToString("N"),
            "DigitalEntitlements");
        var injected = Path.Combine(quarantinedEntitlements, "injected.dat");
        File.WriteAllText(injected, "newer");

        await Assert.ThrowsAsync<IOException>(() => fixture.Action.CommitAsync(
            fixture.Context,
            result.SnapshotJson,
            CancellationToken.None));

        Assert.Equal("newer", File.ReadAllText(injected));
        Assert.Equal("token", File.ReadAllText(Path.Combine(quarantinedEntitlements, "token.dat")));
    }

    [Fact]
    public async Task Commit_RejectsReparsePointInjectedIntoQuarantinedDirectoryWhenSupported()
    {
        using var fixture = new Fixture("entitlement error");
        var result = await fixture.Action.ApplyAsync(fixture.Context, CancellationToken.None);
        var quarantinedEntitlements = Path.Combine(
            fixture.QuarantineRoot,
            fixture.Context.TransactionId.ToString("N"),
            "DigitalEntitlements");
        var outside = fixture.TemporaryDirectory.Combine("outside");
        Directory.CreateDirectory(outside);
        var outsideFile = Path.Combine(outside, "keep.txt");
        File.WriteAllText(outsideFile, "keep");
        try
        {
            Directory.CreateSymbolicLink(Path.Combine(quarantinedEntitlements, "linked"), outside);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
            or IOException
            or PlatformNotSupportedException)
        {
            return;
        }

        await Assert.ThrowsAsync<IOException>(() => fixture.Action.CommitAsync(
            fixture.Context,
            result.SnapshotJson,
            CancellationToken.None));

        Assert.Equal("keep", File.ReadAllText(outsideFile));
    }

    private sealed class Fixture : IDisposable
    {
        public Fixture(string? log, bool createItems = true)
        {
            TemporaryDirectory = new TemporaryDirectory();
            var localRoot = TemporaryDirectory.Combine("Local");
            InstallationRoot = Path.Combine(localRoot, "FiveM");
            FiveMAppRoot = Path.Combine(InstallationRoot, "FiveM.app");
            var logsRoot = Path.Combine(FiveMAppRoot, "logs");
            RosIdPath = TemporaryDirectory.Combine("Roaming", "CitizenFX", "ros_id.dat");
            DigitalEntitlementsRoot = Path.Combine(localRoot, "DigitalEntitlements");
            QuarantineRoot = TemporaryDirectory.Combine("Ralven", "AuthQuarantine");
            Directory.CreateDirectory(logsRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(RosIdPath)!);
            if (log is not null)
            {
                LogPath = Path.Combine(logsRoot, "CitizenFX.log");
                File.WriteAllText(LogPath, log);
            }

            if (createItems)
            {
                Directory.CreateDirectory(DigitalEntitlementsRoot);
                File.WriteAllText(RosIdPath, "ros");
                File.WriteAllText(Path.Combine(DigitalEntitlementsRoot, "token.dat"), "token");
            }
            Context = new WindowsActionContext
            {
                TransactionId = Guid.NewGuid(),
                StartedAtUtc = DateTimeOffset.UtcNow,
                IsElevated = false
            };
            Action = new StaleAuthDataRepairAction(
                FiveMAppRoot,
                InstallationRoot,
                RosIdPath,
                Path.GetDirectoryName(RosIdPath)!,
                DigitalEntitlementsRoot,
                Path.GetDirectoryName(DigitalEntitlementsRoot)!,
                QuarantineRoot,
                new FakeProcessInspector());
        }

        public TemporaryDirectory TemporaryDirectory { get; }

        public StaleAuthDataRepairAction Action { get; }

        public WindowsActionContext Context { get; }

        public string FiveMAppRoot { get; }

        public string InstallationRoot { get; }

        public string LogPath { get; } = string.Empty;

        public string RosIdPath { get; }

        public string DigitalEntitlementsRoot { get; }

        public string QuarantineRoot { get; }

        public void Dispose() => TemporaryDirectory.Dispose();
    }
}
