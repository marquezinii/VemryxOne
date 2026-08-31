using System.Text.Json;
using System.Text.Json.Nodes;
using Ralven.Contracts;
using Ralven.Windows.Actions;
using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.Windows;

public sealed class GtaVLaunchParametersDiagnosisActionTests
{
    [Fact]
    public async Task ApplyAsync_WarnsWhenRepairFlagsAreStillActive()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        File.WriteAllLines(
            Path.Combine(gtaVRoot, "commandline.txt"),
            ["-fullscreen", "-safemode", "-cityDensity 0.550000"]);
        var action = new GtaVLaunchParametersDiagnosisAction(gtaVRoot);

        var result = await action.ApplyAsync(Context(), CancellationToken.None);

        Assert.False(result.Changed);
        Assert.Contains("reparo", result.Messages.Single(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("-safemode", result.Messages.Single(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApplyAsync_ReportsNoFileWhenAbsent()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var action = new GtaVLaunchParametersDiagnosisAction(gtaVRoot);

        var result = await action.ApplyAsync(Context(), CancellationToken.None);

        Assert.False(result.Changed);
        Assert.Equal(ActionExecutionOutcome.Skipped, result.Outcome);
        Assert.Contains("padrão", result.Messages.Single(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApplyAsync_ReportsUnconfirmedInstallation()
    {
        var action = new GtaVLaunchParametersDiagnosisAction(gtaVInstallationRoot: null);

        var result = await action.ApplyAsync(Context(), CancellationToken.None);

        Assert.False(result.Changed);
        Assert.Equal(ActionExecutionOutcome.Skipped, result.Outcome);
        Assert.Contains("não foi confirmada", result.Messages.Single(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApplyAsync_SkipsWhenCommandLineCannotBeRead()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var commandLinePath = Path.Combine(gtaVRoot, "commandline.txt");
        File.WriteAllLines(commandLinePath, ["-fullscreen"]);
        await using var locked = new FileStream(
            commandLinePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var action = new GtaVLaunchParametersDiagnosisAction(gtaVRoot);

        var result = await action.ApplyAsync(Context(), CancellationToken.None);

        Assert.Equal(ActionExecutionOutcome.Skipped, result.Outcome);
    }

    private static WindowsActionContext Context()
    {
        return new WindowsActionContext
        {
            TransactionId = Guid.NewGuid(),
            StartedAtUtc = DateTimeOffset.UtcNow,
            IsElevated = false
        };
    }
}

public sealed class GtaVGraphicsLaunchParametersActionTests
{
    [Fact]
    public async Task ApplyAsync_WritesManagedLinesAndPreservesUnknownOnesThenRollsBackExactly()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var commandLinePath = Path.Combine(gtaVRoot, "commandline.txt");
        File.WriteAllLines(commandLinePath, ["-scOfflineOnly", "-cityDensity 0.100000"]);
        var display = new FakeDisplayConfigurationInspector();
        var gtaVInspector = new FakeGtaVProcessInspector();
        var action = new GtaVGraphicsLaunchParametersAction(gtaVRoot, display, gtaVInspector);
        var context = Context();

        var result = await action.ApplyAsync(context, CancellationToken.None);

        Assert.True(result.Changed);
        var lines = File.ReadAllLines(commandLinePath);
        Assert.Contains("-scOfflineOnly", lines);
        Assert.Contains("-cityDensity 0.550000", lines);
        Assert.Contains("-fxaa", lines);
        Assert.Contains("-frameLimit 144", lines);
        Assert.DoesNotContain("-cityDensity 0.100000", lines);

        await action.RollbackAsync(context, result.SnapshotJson, CancellationToken.None);

        Assert.Equal(["-scOfflineOnly", "-cityDensity 0.100000"], File.ReadAllLines(commandLinePath));
    }

    [Fact]
    public async Task ApplyAsync_NoChangeWhenAlreadyAtDesiredValues()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var commandLinePath = Path.Combine(gtaVRoot, "commandline.txt");
        File.WriteAllLines(
            commandLinePath,
            ["-cityDensity 0.550000", "-anisotropicQualityLevel 8", "-fxaa", "-grassQuality 1", "-lodScale 0.700000", "-frameLimit 144"]);
        var action = new GtaVGraphicsLaunchParametersAction(
            gtaVRoot, new FakeDisplayConfigurationInspector(), new FakeGtaVProcessInspector());

        var result = await action.ApplyAsync(Context(), CancellationToken.None);

        Assert.False(result.Changed);
        Assert.Equal(ActionExecutionOutcome.Verified, result.Outcome);
    }

    [Fact]
    public async Task ApplyAsync_SkipsWhenInstallationIsUnconfirmed()
    {
        var action = new GtaVGraphicsLaunchParametersAction(
            gtaVInstallationRoot: null,
            new FakeDisplayConfigurationInspector(),
            new FakeGtaVProcessInspector());

        var result = await action.ApplyAsync(Context(), CancellationToken.None);

        Assert.Equal(ActionExecutionOutcome.Skipped, result.Outcome);
    }

    [Fact]
    public async Task ApplyAsync_RefusesToWriteWhileGtaVIsRunning()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var action = new GtaVGraphicsLaunchParametersAction(
            gtaVRoot, new FakeDisplayConfigurationInspector(), new FakeGtaVProcessInspector(running: true));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            action.ApplyAsync(Context(), CancellationToken.None));
    }

    [Fact]
    public async Task ApplyAsync_ThrowsWhenCommandLineCannotBeRead()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var commandLinePath = Path.Combine(gtaVRoot, "commandline.txt");
        File.WriteAllLines(commandLinePath, ["-cityDensity 0.100000"]);
        await using var locked = new FileStream(
            commandLinePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var action = new GtaVGraphicsLaunchParametersAction(
            gtaVRoot, new FakeDisplayConfigurationInspector(), new FakeGtaVProcessInspector());

        await Assert.ThrowsAsync<IOException>(() =>
            action.ApplyAsync(Context(), CancellationToken.None));
    }

    [Fact]
    public async Task RollbackAsync_RefusesToOverwriteNewerUserEdit()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var commandLinePath = Path.Combine(gtaVRoot, "commandline.txt");
        File.WriteAllLines(commandLinePath, ["-cityDensity 0.100000"]);
        var action = new GtaVGraphicsLaunchParametersAction(
            gtaVRoot, new FakeDisplayConfigurationInspector(), new FakeGtaVProcessInspector());
        var context = Context();
        var result = await action.ApplyAsync(context, CancellationToken.None);
        File.WriteAllLines(commandLinePath, ["-newer-user-setting"]);

        await Assert.ThrowsAsync<IOException>(() =>
            action.RollbackAsync(context, result.SnapshotJson, CancellationToken.None));

        Assert.Equal(["-newer-user-setting"], File.ReadAllLines(commandLinePath));
    }

    [Fact]
    public async Task RollbackAsync_RejectsInjectedPathWithoutTouchingAnyFile()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var commandLinePath = Path.Combine(gtaVRoot, "commandline.txt");
        var outsidePath = temporaryDirectory.Combine("outside.txt");
        File.WriteAllLines(commandLinePath, ["-cityDensity 0.100000"]);
        File.WriteAllLines(outsidePath, ["preserve"]);
        var action = new GtaVGraphicsLaunchParametersAction(
            gtaVRoot, new FakeDisplayConfigurationInspector(), new FakeGtaVProcessInspector());
        var context = Context();
        var result = await action.ApplyAsync(context, CancellationToken.None);
        var appliedLines = File.ReadAllLines(commandLinePath);
        var snapshot = JsonNode.Parse(result.SnapshotJson!)!.AsObject();
        snapshot["settingsPath"] = outsidePath;

        await Assert.ThrowsAsync<JsonException>(() =>
            action.RollbackAsync(context, snapshot.ToJsonString(), CancellationToken.None));

        Assert.Equal(appliedLines, File.ReadAllLines(commandLinePath));
        Assert.Equal(["preserve"], File.ReadAllLines(outsidePath));
    }

    [Fact]
    public void Postcondition_RejectsIgnoredWrite()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var commandLinePath = temporaryDirectory.Combine("commandline.txt");
        File.WriteAllLines(commandLinePath, ["-old"]);

        Assert.Throws<IOException>(() =>
            GtaVCommandLineFile.EnsureExpectedContents(commandLinePath, ["-new"]));
    }

    [Fact]
    public async Task RollbackAsync_RemovesFileThatDidNotExistBeforeApply()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var commandLinePath = Path.Combine(gtaVRoot, "commandline.txt");
        var action = new GtaVGraphicsLaunchParametersAction(
            gtaVRoot, new FakeDisplayConfigurationInspector(), new FakeGtaVProcessInspector());
        var context = Context();
        var result = await action.ApplyAsync(context, CancellationToken.None);

        await action.RollbackAsync(context, result.SnapshotJson, CancellationToken.None);

        Assert.False(File.Exists(commandLinePath));
    }

    [Fact]
    public void WriteAtomically_PreservesEditMadeAfterInitialRead()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var commandLinePath = temporaryDirectory.Combine("commandline.txt");
        File.WriteAllLines(commandLinePath, ["-original"]);
        var originalSha256 = SafeXmlDocumentStore.ComputeSha256(commandLinePath);
        File.WriteAllLines(commandLinePath, ["-newer-user-setting"]);

        Assert.Throws<IOException>(() => GtaVCommandLineFile.WriteAtomically(
            commandLinePath,
            ["-ralven-setting"],
            originalExisted: true,
            expectedOriginalSha256: originalSha256));

        Assert.Equal(["-newer-user-setting"], File.ReadAllLines(commandLinePath));
    }

    [Fact]
    public void CompensateFailedApply_RestoresDisplacedOriginal()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var commandLinePath = temporaryDirectory.Combine("commandline.txt");
        var displacedPath = temporaryDirectory.Combine("commandline.displaced");
        File.WriteAllLines(commandLinePath, ["-applied"]);
        File.WriteAllLines(displacedPath, ["-original"]);
        var appliedSha256 = SafeXmlDocumentStore.ComputeSha256(commandLinePath);

        GtaVCommandLineFile.CompensateFailedApply(
            commandLinePath,
            originalExisted: true,
            displacedPath: displacedPath,
            appliedSha256: appliedSha256);

        Assert.Equal(["-original"], File.ReadAllLines(commandLinePath));
        Assert.False(File.Exists(displacedPath));
    }

    [Fact]
    public async Task ApplyAsync_RejectsReparsePointInInstallationAncestors()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var realRoot = temporaryDirectory.Combine("real-gta");
        var linkedRoot = temporaryDirectory.Combine("linked-gta");
        Directory.CreateDirectory(realRoot);
        var commandLinePath = Path.Combine(realRoot, "commandline.txt");
        File.WriteAllLines(commandLinePath, ["-original"]);
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

        var action = new GtaVGraphicsLaunchParametersAction(
            linkedRoot, new FakeDisplayConfigurationInspector(), new FakeGtaVProcessInspector());

        await Assert.ThrowsAsync<IOException>(() =>
            action.ApplyAsync(Context(), CancellationToken.None));

        Assert.Equal(["-original"], File.ReadAllLines(commandLinePath));
    }

    [Fact]
    public async Task RollbackAsync_RejectsLegacySnapshotWithoutProvableAppliedState()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var commandLinePath = Path.Combine(gtaVRoot, "commandline.txt");
        File.WriteAllLines(commandLinePath, ["-newer-user-setting"]);
        var action = new GtaVGraphicsLaunchParametersAction(
            gtaVRoot, new FakeDisplayConfigurationInspector(), new FakeGtaVProcessInspector());
        var legacySnapshot = JsonSerializer.Serialize(new
        {
            settingsPath = commandLinePath,
            originalExisted = true,
            originalLines = new[] { "-old-setting" },
            changedFlags = new[] { "-cityDensity" }
        });

        var error = await Assert.ThrowsAsync<InvalidDataException>(() =>
            action.RollbackAsync(Context(), legacySnapshot, CancellationToken.None));

        Assert.Contains("snapshot legado", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(["-newer-user-setting"], File.ReadAllLines(commandLinePath));
    }

    private static WindowsActionContext Context()
    {
        return new WindowsActionContext
        {
            TransactionId = Guid.NewGuid(),
            StartedAtUtc = DateTimeOffset.UtcNow,
            IsElevated = false
        };
    }
}

public sealed class GtaVDisplayLaunchParametersActionTests
{
    [Theory]
    [InlineData(false, false, "-fullscreen")]
    [InlineData(true, false, "-windowed")]
    [InlineData(true, true, "-borderless")]
    public async Task ApplyAsync_WritesMutuallyExclusiveDisplayMode(
        bool windowed, bool borderless, string expectedFlag)
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var commandLinePath = Path.Combine(gtaVRoot, "commandline.txt");
        File.WriteAllLines(commandLinePath, ["-fullscreen"]);
        var action = new GtaVDisplayLaunchParametersAction(
            gtaVRoot, windowed, borderless, GtaVDirectXVersion.Unspecified, new FakeGtaVProcessInspector());

        var result = await action.ApplyAsync(Context(), CancellationToken.None);

        var lines = File.ReadAllLines(commandLinePath);
        Assert.Contains(expectedFlag, lines);
        Assert.Single(lines, line => line is "-fullscreen" or "-windowed" or "-borderless");
        _ = result;
    }

    [Fact]
    public async Task ApplyAsync_WritesChosenDirectXVersionAndRemovesOthers()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var commandLinePath = Path.Combine(gtaVRoot, "commandline.txt");
        File.WriteAllLines(commandLinePath, ["-DX10"]);
        var action = new GtaVDisplayLaunchParametersAction(
            gtaVRoot, false, false, GtaVDirectXVersion.DX11, new FakeGtaVProcessInspector());

        await action.ApplyAsync(Context(), CancellationToken.None);

        var lines = File.ReadAllLines(commandLinePath);
        Assert.Contains("-DX11", lines);
        Assert.DoesNotContain("-DX10", lines);
    }

    private static WindowsActionContext Context()
    {
        return new WindowsActionContext
        {
            TransactionId = Guid.NewGuid(),
            StartedAtUtc = DateTimeOffset.UtcNow,
            IsElevated = false
        };
    }
}

public sealed class GtaVRepairLaunchParametersActionTests
{
    [Fact]
    public async Task ApplyAsync_OnlyWritesRequestedRepairFlagsAndRollsBackExactly()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var commandLinePath = Path.Combine(gtaVRoot, "commandline.txt");
        File.WriteAllLines(commandLinePath, ["-fullscreen"]);
        var action = new GtaVRepairLaunchParametersAction(
            gtaVRoot,
            useSafeMode: true,
            useMinimumSettings: false,
            useAutoSettingsRebuild: false,
            new FakeGtaVProcessInspector());
        var context = Context();

        var result = await action.ApplyAsync(context, CancellationToken.None);

        Assert.True(result.Changed);
        var lines = File.ReadAllLines(commandLinePath);
        Assert.Contains("-fullscreen", lines);
        Assert.Contains("-safemode", lines);
        Assert.DoesNotContain("-useMinimumSettings", lines);
        Assert.DoesNotContain("-UseAutoSettings", lines);

        await action.RollbackAsync(context, result.SnapshotJson, CancellationToken.None);

        Assert.Equal(["-fullscreen"], File.ReadAllLines(commandLinePath));
    }

    [Fact]
    public async Task ApplyAsync_NoChangeWhenNoRepairFlagsRequestedAndFileHasNone()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var gtaVRoot = temporaryDirectory.Combine("Grand Theft Auto V");
        Directory.CreateDirectory(gtaVRoot);
        var action = new GtaVRepairLaunchParametersAction(
            gtaVRoot, false, false, false, new FakeGtaVProcessInspector());

        var result = await action.ApplyAsync(Context(), CancellationToken.None);

        Assert.False(result.Changed);
        Assert.False(File.Exists(Path.Combine(gtaVRoot, "commandline.txt")));
    }

    private static WindowsActionContext Context()
    {
        return new WindowsActionContext
        {
            TransactionId = Guid.NewGuid(),
            StartedAtUtc = DateTimeOffset.UtcNow,
            IsElevated = false
        };
    }
}
