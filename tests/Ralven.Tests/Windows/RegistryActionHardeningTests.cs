using Microsoft.Win32;
using Ralven.Windows.Actions;
using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.Windows;

public sealed class RegistryActionHardeningTests
{
    [Fact]
    public async Task Apply_WhenWriteIsIgnored_RestoresAndFails()
    {
        var registry = RegistryWithGameModeDisabled();
        registry.WriteBehaviors.Enqueue(WriteBehavior.Ignore);
        registry.WriteBehaviors.Enqueue(WriteBehavior.Normal);
        var action = new GameModeRegistryAction(registry, new FakeProcessInspector());

        await Assert.ThrowsAsync<IOException>(() =>
            action.ApplyAsync(Context(), CancellationToken.None));

        Assert.Equal(0, registry.Read(GameModeAddress).NumericValue);
    }

    [Fact]
    public async Task Apply_WhenWriteMutatesThenThrows_RestoresAndPreservesOriginalFailure()
    {
        var registry = RegistryWithGameModeDisabled();
        registry.WriteBehaviors.Enqueue(WriteBehavior.MutateThenThrow);
        registry.WriteBehaviors.Enqueue(WriteBehavior.Normal);
        var action = new GameModeRegistryAction(registry, new FakeProcessInspector());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            action.ApplyAsync(Context(), CancellationToken.None));

        Assert.Equal(ScriptedRegistryStore.MutationFailureMessage, exception.Message);
        Assert.Equal(0, registry.Read(GameModeAddress).NumericValue);
    }

    [Fact]
    public async Task Apply_WhenMutationAndRecoveryFail_PreservesBothFailures()
    {
        var registry = RegistryWithGameModeDisabled();
        registry.WriteBehaviors.Enqueue(WriteBehavior.MutateThenThrow);
        registry.WriteBehaviors.Enqueue(WriteBehavior.Ignore);
        var action = new GameModeRegistryAction(registry, new FakeProcessInspector());

        var exception = await Assert.ThrowsAsync<AggregateException>(() =>
            action.ApplyAsync(Context(), CancellationToken.None));

        Assert.Collection(
            exception.InnerExceptions,
            original => Assert.Equal(ScriptedRegistryStore.MutationFailureMessage, original.Message),
            recovery => Assert.IsType<IOException>(recovery));
        Assert.Equal(1, registry.Read(GameModeAddress).NumericValue);
    }

    [Fact]
    public async Task Apply_WhenValueChangesAgainDuringFailure_PreservesConcurrentValue()
    {
        var registry = RegistryWithGameModeDisabled();
        registry.WriteBehaviors.Enqueue(WriteBehavior.ConcurrentChangeThenThrow);
        var action = new GameModeRegistryAction(registry, new FakeProcessInspector());

        var exception = await Assert.ThrowsAsync<AggregateException>(() =>
            action.ApplyAsync(Context(), CancellationToken.None));

        Assert.Collection(
            exception.InnerExceptions,
            original => Assert.Equal(ScriptedRegistryStore.MutationFailureMessage, original.Message),
            recovery => Assert.IsType<IOException>(recovery));
        Assert.Equal(7, registry.Read(GameModeAddress).NumericValue);
        Assert.Equal(1, registry.WriteCount);
    }

    [Fact]
    public async Task Rollback_WhenRestoreIsIgnored_FailsInsteadOfReportingSuccess()
    {
        var registry = RegistryWithGameModeDisabled();
        var action = new GameModeRegistryAction(registry, new FakeProcessInspector());
        var context = Context();
        var applied = await action.ApplyAsync(context, CancellationToken.None);
        registry.WriteBehaviors.Enqueue(WriteBehavior.Ignore);

        await Assert.ThrowsAsync<IOException>(() =>
            action.RollbackAsync(context, applied.SnapshotJson, CancellationToken.None));

        Assert.Equal(1, registry.Read(GameModeAddress).NumericValue);
    }

    [Fact]
    public async Task Rollback_WhenValueChangedAfterApply_FailsBeforeWriting()
    {
        var registry = RegistryWithGameModeDisabled();
        var action = new GameModeRegistryAction(registry, new FakeProcessInspector());
        var context = Context();
        var applied = await action.ApplyAsync(context, CancellationToken.None);
        registry.Seed(GameModeAddress, RegistryValueState.FromDword(2));

        await Assert.ThrowsAsync<IOException>(() =>
            action.RollbackAsync(context, applied.SnapshotJson, CancellationToken.None));

        Assert.Equal(2, registry.Read(GameModeAddress).NumericValue);
        Assert.Equal(1, registry.WriteCount);
    }

    [Theory]
    [InlineData(RegistryValueKind.String, 0)]
    [InlineData(RegistryValueKind.DWord, 0)]
    [InlineData(RegistryValueKind.DWord, 3)]
    public async Task Hags_RejectsExistingValueOutsideSupportedDomain(
        RegistryValueKind kind,
        long numericValue)
    {
        var registry = new ScriptedRegistryStore();
        var value = kind == RegistryValueKind.String
            ? RegistryValueState.FromString("2")
            : RegistryValueState.FromDword((int)numericValue);
        registry.Seed(HagsAddress, value);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new HagsToggleAction(registry).ApplyAsync(Context(elevated: true), CancellationToken.None));

        Assert.Equal(value, registry.Read(HagsAddress));
        Assert.Equal(0, registry.WriteCount);
    }

    [Fact]
    public async Task HagsRollback_RejectsUnsupportedPreviousValueInSnapshot()
    {
        var registry = new ScriptedRegistryStore();
        registry.Seed(HagsAddress, RegistryValueState.FromDword(2));
        var snapshot = WindowsActionSnapshot.Serialize(new RegistryMutationSnapshot(
        [
            new RegistryMutationSnapshotEntry(
                HagsAddress,
                RegistryValueState.FromDword(3),
                RegistryValueState.FromDword(2))
        ]));

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new HagsToggleAction(registry).RollbackAsync(
                Context(elevated: true),
                snapshot,
                CancellationToken.None));

        Assert.Equal(2, registry.Read(HagsAddress).NumericValue);
        Assert.Equal(0, registry.WriteCount);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task GamingRollback_RejectsUnsupportedPreviousValueInSnapshot(
        bool gameMode,
        bool wrongType)
    {
        var registry = new ScriptedRegistryStore();
        var address = gameMode ? GameModeAddress : GameDvrAddress;
        var appliedValue = RegistryValueState.FromDword(gameMode ? 1 : 0);
        registry.Seed(address, appliedValue);
        var snapshot = WindowsActionSnapshot.Serialize(new RegistryMutationSnapshot(
        [
            new RegistryMutationSnapshotEntry(
                address,
                wrongType
                    ? RegistryValueState.FromString("unsupported")
                    : RegistryValueState.FromDword(7),
                appliedValue)
        ]));
        WindowsOptimizationAction action = gameMode
            ? new GameModeRegistryAction(registry, new FakeProcessInspector())
            : new GameDvrRegistryAction(registry, new FakeProcessInspector());

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            action.RollbackAsync(Context(), snapshot, CancellationToken.None));

        Assert.Equal(appliedValue, registry.Read(address));
        Assert.Equal(0, registry.WriteCount);
    }

    [Fact]
    public async Task Rollback_WhenSecondValueChangesDuringRestore_CompensatesFirstValue()
    {
        var registry = new ScriptedRegistryStore();
        var fiveM = Path.GetFullPath("FiveM.exe");
        var gtaV = Path.GetFullPath("GTA5.exe");
        var fiveMAddress = FullscreenAddress(fiveM);
        var gtaVAddress = FullscreenAddress(gtaV);
        var action = new FullscreenOptimizationsRegistryAction(registry, fiveM, gtaV);
        var context = Context();
        var applied = await action.ApplyAsync(context, CancellationToken.None);
        registry.ResetReadCount();
        registry.OnRead = (readCount, address, values) =>
        {
            if (readCount == 5 && address == fiveMAddress)
            {
                values.Seed(address, RegistryValueState.FromString("EXTERNAL"));
            }
        };

        await Assert.ThrowsAsync<IOException>(() =>
            action.RollbackAsync(context, applied.SnapshotJson, CancellationToken.None));

        Assert.Equal("EXTERNAL", registry.Read(fiveMAddress).StringValue);
        Assert.Equal(FullscreenDisableFlag, registry.Read(gtaVAddress).StringValue);
    }

    [Fact]
    public async Task FullscreenToggle_EmptyStringAddsDisableFlagAgain()
    {
        var registry = new ScriptedRegistryStore();
        var executable = Path.GetFullPath("FiveM.exe");
        var address = FullscreenAddress(executable);
        registry.Seed(address, RegistryValueState.FromString(FullscreenDisableFlag));
        var action = new FullscreenOptimizationsRegistryAction(registry, executable, null);

        await action.ApplyAsync(Context(), CancellationToken.None);
        Assert.Equal(string.Empty, registry.Read(address).StringValue);

        await action.ApplyAsync(Context(), CancellationToken.None);
        Assert.Equal(FullscreenDisableFlag, registry.Read(address).StringValue);
    }

    private static readonly RegistryAddress GameModeAddress = new(
        RegistryHive.CurrentUser,
        @"Software\Microsoft\GameBar",
        "AutoGameModeEnabled");

    private static readonly RegistryAddress HagsAddress = new(
        RegistryHive.LocalMachine,
        @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
        "HwSchMode");

    private static readonly RegistryAddress GameDvrAddress = new(
        RegistryHive.CurrentUser,
        @"Software\Microsoft\Windows\CurrentVersion\GameDVR",
        "HistoricalCaptureEnabled");

    private const string FullscreenDisableFlag = "DISABLEDXMAXIMIZEDWINDOWEDMODE";

    private static RegistryAddress FullscreenAddress(string executable) => new(
        RegistryHive.CurrentUser,
        @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers",
        executable);

    private static ScriptedRegistryStore RegistryWithGameModeDisabled()
    {
        var registry = new ScriptedRegistryStore();
        registry.Seed(GameModeAddress, RegistryValueState.FromDword(0));
        return registry;
    }

    private static WindowsActionContext Context(bool elevated = false) => new()
    {
        TransactionId = Guid.NewGuid(),
        StartedAtUtc = DateTimeOffset.UtcNow,
        IsElevated = elevated
    };

    private enum WriteBehavior
    {
        Normal,
        Ignore,
        MutateThenThrow,
        ConcurrentChangeThenThrow
    }

    private sealed class ScriptedRegistryStore : IRegistryStore
    {
        internal const string MutationFailureMessage = "scripted mutation failure";

        private readonly Dictionary<RegistryAddress, RegistryValueState> values = new();

        public Queue<WriteBehavior> WriteBehaviors { get; } = new();

        public int WriteCount { get; private set; }

        public int ReadCount { get; private set; }

        public Action<int, RegistryAddress, ScriptedRegistryStore>? OnRead { get; set; }

        public RegistryValueState Read(RegistryAddress address)
        {
            ReadCount++;
            OnRead?.Invoke(ReadCount, address, this);
            return values.TryGetValue(address, out var value) ? value : RegistryValueState.Missing;
        }

        public void Write(RegistryAddress address, RegistryValueState state)
        {
            WriteCount++;
            var behavior = WriteBehaviors.TryDequeue(out var scripted)
                ? scripted
                : WriteBehavior.Normal;
            if (behavior == WriteBehavior.Ignore)
            {
                return;
            }

            values[address] = state;
            if (behavior is WriteBehavior.MutateThenThrow
                or WriteBehavior.ConcurrentChangeThenThrow)
            {
                if (behavior == WriteBehavior.ConcurrentChangeThenThrow)
                {
                    values[address] = RegistryValueState.FromDword(7);
                }

                throw new InvalidOperationException(MutationFailureMessage);
            }
        }

        public void Delete(RegistryAddress address) => values.Remove(address);

        public void Seed(RegistryAddress address, RegistryValueState state) => values[address] = state;

        public void ResetReadCount() => ReadCount = 0;
    }
}
