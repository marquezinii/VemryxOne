using Ralven.App.Services;
using Ralven.App.ViewModels;
using Ralven.Contracts;
using Ralven.Windows.Actions;
using Ralven.Windows.Engine;
using Ralven.Windows.Infrastructure;
using Ralven.Tests.Windows;
using Xunit;

namespace Ralven.Tests.App;

public sealed class WindowsGamingControlsServiceTests
{
    private static DownloadedUpdate CreateDownload(string version = "9.9.9") => new(
        StableSemanticVersion.Parse(version),
        InstallerPath: $@"C:\Updates\Ralven-Setup-{version}-win-x64.exe",
        SizeBytes: 10 * 1024 * 1024,
        Sha256Hex: new string('a', 64),
        WasAlreadyDownloaded: false);

    private static WindowsGamingControlsService CreateService(
        IRegistryStore registry,
        IWindowsTransactionJournalStore journals,
        IFiveMProcessInspector? processInspector = null)
    {
        return new WindowsGamingControlsService(
            registry,
            journals,
            processInspector ?? new FakeProcessInspector());
    }

    [Fact]
    public async Task DemoRead_UsesConfiguredStateSoTheNonMutatingPreviewCannotApply()
    {
        var service = new WindowsGamingControlsService(demoMode: true);

        var result = await service.ReadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(WindowsGamingSettingState.Enabled, result.GameMode);
        Assert.Equal(WindowsGamingSettingState.Disabled, result.BackgroundCapture);
    }

    [Fact]
    public async Task DemoViewModel_DisablesApplyInsteadOfOfferingAnOperationThatWouldFail()
    {
        using var viewModel = new MainViewModel(
            new FakeAppOptimizationService(new AppSettings(), settingsFileExists: false),
            windowsGamingControls: new WindowsGamingControlsService(demoMode: true));

        await viewModel.InitializeAsync();
        await viewModel.RefreshWindowsGamingSettingsAsync();

        Assert.False(viewModel.CanApplyWindowsGamingSettings);
        Assert.False(viewModel.CanRestoreWindowsGamingSettings);
    }

    [Fact]
    public async Task ApplyAndRestore_UsesOnlyTheTwoTypedActionsAndRestoresExactValues()
    {
        var registry = new FakeRegistryStore();
        var journals = new InMemoryJournalStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        var service = CreateService(registry, journals);

        var applied = await service.ApplyAsync(TestContext.Current.CancellationToken);

        Assert.True(applied.Succeeded);
        Assert.True(applied.Changed);
        Assert.Equal(WindowsGamingSettingState.Enabled, applied.Settings.GameMode);
        Assert.Equal(WindowsGamingSettingState.Disabled, applied.Settings.BackgroundCapture);
        Assert.Equal(
            [
                "windows.gaming.game-mode.enable",
                "windows.gaming.background-capture.disable"
            ],
            journals.Get(applied.TransactionId).Actions.Select(action => action.ActionId));

        var restoration = await service.RestoreAsync(
            applied.TransactionId,
            TestContext.Current.CancellationToken);
        Assert.True(restoration.Succeeded);

        var restored = await service.ReadAsync(TestContext.Current.CancellationToken);
        Assert.Equal(WindowsGamingSettingState.Disabled, restored.GameMode);
        Assert.Equal(WindowsGamingSettingState.Enabled, restored.BackgroundCapture);
    }

    [Fact]
    public async Task Apply_RefusesToOverwriteAnUnexpectedRegistryShape()
    {
        var registry = new FakeRegistryStore();
        var service = CreateService(registry, new InMemoryJournalStore());
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromString("unexpected"));

        var result = await service.ApplyAsync(TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.False(result.Changed);
        Assert.Equal(Guid.Empty, result.TransactionId);
        Assert.Equal(WindowsGamingSettingState.Unavailable, result.Settings.GameMode);
        Assert.Equal(WindowsGamingSettingState.NotConfigured, result.Settings.BackgroundCapture);
    }

    [Fact]
    public async Task Apply_RecognizesDifferentlyCasedRegistryValueAndPreservesItsShape()
    {
        var registry = new FakeRegistryStore();
        var differentlyCasedAddress = new RegistryAddress(
            GameModeRegistryAction.Address.Hive,
            GameModeRegistryAction.Address.SubKey.ToUpperInvariant(),
            GameModeRegistryAction.Address.ValueName.ToLowerInvariant());
        registry.Write(differentlyCasedAddress, RegistryValueState.FromString("unexpected"));
        var service = CreateService(registry, new InMemoryJournalStore());

        var result = await service.ApplyAsync(TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal(Guid.Empty, result.TransactionId);
        Assert.Equal(
            "unexpected",
            registry.Read(differentlyCasedAddress).StringValue);
    }

    [Fact]
    public async Task Apply_DoesNotReloadTheJournalAfterTheEngineCommits()
    {
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        var journals = new ThrowOnSecondLoadJournalStore();
        var service = CreateService(registry, journals);

        var result = await service.ApplyAsync(TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        Assert.True(result.Changed);
        Assert.Equal(1, journals.LoadCount);
    }

    [Fact]
    public async Task Apply_WhenJournalPersistenceFailsAfterAWrite_UsesEmergencyRollback()
    {
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        var service = CreateService(registry, new FailAfterActionWriteJournalStore());

        var result = await service.ApplyAsync(TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.True(result.Changed);
        Assert.Equal(0, registry.Read(GameModeRegistryAction.Address).NumericValue);
        Assert.Equal(
            1,
            registry.Read(GameDvrRegistryAction.HistoricalCaptureAddress).NumericValue);
    }

    [Fact]
    public async Task Apply_WhenFiveMIsRunning_BlocksBeforeCreatingAJournal()
    {
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        var journals = new InMemoryJournalStore();
        var service = CreateService(
            registry,
            journals,
            new FakeProcessInspector(running: true));

        var result = await service.ApplyAsync(TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal(Guid.Empty, result.TransactionId);
        Assert.Equal(WindowsGamingControlsBlockReason.FiveMRunning, result.BlockReason);
        Assert.Equal(0, registry.Read(GameModeRegistryAction.Address).NumericValue);
        Assert.Equal(
            1,
            registry.Read(GameDvrRegistryAction.HistoricalCaptureAddress).NumericValue);
    }

    [Fact]
    public async Task Apply_WhenProcessInspectionFails_BlocksWithoutWriting()
    {
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        var service = CreateService(
            registry,
            new InMemoryJournalStore(),
            new ThrowingProcessInspector());

        var result = await service.ApplyAsync(TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal(Guid.Empty, result.TransactionId);
        Assert.Equal(
            WindowsGamingControlsBlockReason.ProcessInspectionUnavailable,
            result.BlockReason);
        Assert.Equal(0, registry.Read(GameModeRegistryAction.Address).NumericValue);
    }

    [Fact]
    public async Task Apply_WhenFiveMStartsBetweenActions_RestoresTheSameRunImmediately()
    {
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        var journals = new InMemoryJournalStore();
        var processInspector = new SequencedProcessInspector(false, false, true, true);
        var service = CreateService(registry, journals, processInspector);

        var result = await service.ApplyAsync(TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.True(result.Changed);
        Assert.Equal(WindowsGamingControlsBlockReason.FiveMRunning, result.BlockReason);
        Assert.Equal(TransactionState.RolledBack, journals.Get(result.TransactionId).State);
        Assert.Equal(0, registry.Read(GameModeRegistryAction.Address).NumericValue);
        Assert.Equal(
            1,
            registry.Read(GameDvrRegistryAction.HistoricalCaptureAddress).NumericValue);
    }

    [Fact]
    public async Task Apply_WhenRegistryShapeChangesAfterPreflight_PreservesTheNewValue()
    {
        var values = new FakeRegistryStore();
        values.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        values.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        var registry = new MutatingOnThirdReadRegistryStore(values, inner =>
            inner.Write(GameModeRegistryAction.Address, RegistryValueState.FromString("unexpected")));
        var journals = new InMemoryJournalStore();
        var service = CreateService(registry, journals);

        var result = await service.ApplyAsync(TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.False(result.Changed);
        Assert.Equal("unexpected", values.Read(GameModeRegistryAction.Address).StringValue);
        Assert.Equal(TransactionState.RolledBack, journals.Get(result.TransactionId).State);
    }

    [Fact]
    public async Task Apply_WhenAnotherProcessReachesTheTarget_DoesNotOfferFalseRestore()
    {
        var values = new FakeRegistryStore();
        values.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        values.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        var registry = new MutatingOnThirdReadRegistryStore(values, inner =>
        {
            inner.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(1));
            inner.Write(
                GameDvrRegistryAction.HistoricalCaptureAddress,
                RegistryValueState.FromDword(0));
        });
        var journals = new InMemoryJournalStore();
        var service = CreateService(registry, journals);

        var result = await service.ApplyAsync(TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        Assert.False(result.Changed);
        Assert.All(journals.Get(result.TransactionId).Actions, action => Assert.False(action.Changed));
    }

    [Fact]
    public async Task Apply_WhenSecondActionFails_RollsBackTheFirstAction()
    {
        var values = new FakeRegistryStore();
        var journals = new InMemoryJournalStore();
        values.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        values.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        var registry = new FailHistoricalCaptureWriteRegistryStore(values);
        var service = CreateService(registry, journals);

        var result = await service.ApplyAsync(TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal(TransactionState.RolledBack, journals.Get(result.TransactionId).State);
        Assert.Equal(0, values.Read(GameModeRegistryAction.Address).NumericValue);
        Assert.Equal(1, values.Read(GameDvrRegistryAction.HistoricalCaptureAddress).NumericValue);
    }

    [Fact]
    public async Task ViewModel_WhenRestoreConflicts_KeepsRestoreAvailableForRetry()
    {
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        using var viewModel = new MainViewModel(
            new FakeAppOptimizationService(new AppSettings(), settingsFileExists: false),
            windowsGamingControls: CreateService(
                registry,
                new InMemoryJournalStore()));
        await viewModel.InitializeAsync();
        await viewModel.RefreshWindowsGamingSettingsAsync();
        Assert.True(viewModel.CanApplyWindowsGamingSettings);
        await viewModel.ApplyWindowsGamingSettingsAsync();
        Assert.True(viewModel.CanRestoreWindowsGamingSettings);
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(7));

        await viewModel.RestoreWindowsGamingSettingsAsync();

        Assert.True(viewModel.CanRestoreWindowsGamingSettings);

        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(1));
        await viewModel.RestoreWindowsGamingSettingsAsync();

        Assert.False(viewModel.CanRestoreWindowsGamingSettings);
        Assert.Equal(0, registry.Read(GameModeRegistryAction.Address).NumericValue);
        Assert.Equal(1, registry.Read(GameDvrRegistryAction.HistoricalCaptureAddress).NumericValue);
    }

    [Fact]
    public async Task RollbackFailedJournal_AfterReloadRemainsAvailableForHistoryRetry()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        var journalDirectory = temporaryDirectory.Combine("Transactions");
        var journals = new JsonWindowsTransactionJournalStore(journalDirectory);
        var firstService = CreateService(registry, journals);
        var applied = await firstService.ApplyAsync(TestContext.Current.CancellationToken);
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(7));

        var conflictedRestore = await firstService.RestoreAsync(
            applied.TransactionId,
            TestContext.Current.CancellationToken);
        Assert.False(conflictedRestore.Succeeded);

        var reloadedHistory = await new AppOptimizationService(temporaryDirectory.Path)
            .LoadHistoryAsync(TestContext.Current.CancellationToken);
        var retryable = Assert.Single(reloadedHistory);
        Assert.Equal(AppHistoryKind.WindowsGaming, retryable.Kind);
        Assert.True(retryable.CanRollback);

        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(1));
        var restartedService = CreateService(
            registry,
            new JsonWindowsTransactionJournalStore(journalDirectory));

        var retriedRestore = await restartedService.RestoreAsync(
            retryable.TransactionId,
            TestContext.Current.CancellationToken);
        Assert.True(retriedRestore.Succeeded);
        Assert.Equal(0, registry.Read(GameModeRegistryAction.Address).NumericValue);
        Assert.Equal(1, registry.Read(GameDvrRegistryAction.HistoricalCaptureAddress).NumericValue);
    }

    [Fact]
    public async Task WindowsGamingMutation_DisablesUpdaterAndRejectsDirectInstall()
    {
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        var journals = new BlockingJournalStore(release.Task);
        var update = FakeReleaseUpdateService.CreateUpdate();
        var installer = new FakeSilentUpdateInstaller();
        using var viewModel = new MainViewModel(
            new FakeAppOptimizationService(new AppSettings(), settingsFileExists: false),
            releaseUpdateService: new FakeReleaseUpdateService(updateToReturn: update),
            silentUpdateInstaller: installer,
            windowsGamingControls: CreateService(registry, journals));
        await viewModel.InitializeAsync();
        await viewModel.RefreshWindowsGamingSettingsAsync();
        await viewModel.CheckForUpdatesManuallyAsync();

        var applyTask = viewModel.ApplyWindowsGamingSettingsAsync();
        await journals.SaveStarted.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        Assert.True(viewModel.IsWindowsGamingBusy);
        Assert.False(viewModel.CanDownloadUpdate);
        Assert.False(await viewModel.InstallDownloadedUpdateAsync(CreateDownload()));
        Assert.Equal(0, installer.StartCallCount);

        release.SetResult(true);
        await applyTask;
        Assert.False(viewModel.IsWindowsGamingBusy);
        Assert.True(viewModel.CanDownloadUpdate);
    }

    [Fact]
    public async Task DownloadAndInstallUpdate_DisablesWindowsGamingInBothPhases()
    {
        var update = FakeReleaseUpdateService.CreateUpdate();
        var download = CreateDownload();
        var downloadRelease = new TaskCompletionSource<DownloadedUpdate>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var installRelease = new TaskCompletionSource<SilentUpdateLaunch>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseService = new BlockingDownloadReleaseUpdateService(update, downloadRelease.Task);
        var installer = new BlockingSilentUpdateInstaller(installRelease.Task);
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        using var viewModel = new MainViewModel(
            new FakeAppOptimizationService(new AppSettings(), settingsFileExists: false),
            releaseUpdateService: releaseService,
            silentUpdateInstaller: installer,
            windowsGamingControls: CreateService(
                registry,
                new InMemoryJournalStore()));
        await viewModel.InitializeAsync();
        await viewModel.RefreshWindowsGamingSettingsAsync();
        await viewModel.CheckForUpdatesManuallyAsync();

        var updateTask = viewModel.DownloadAndInstallUpdateAsync();
        await releaseService.DownloadStarted.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
        Assert.True(viewModel.IsUpdateDownloading);
        Assert.False(viewModel.CanApplyWindowsGamingSettings);

        downloadRelease.SetResult(download);
        await installer.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
        Assert.True(viewModel.IsInstallingUpdate);
        Assert.False(viewModel.CanRefreshWindowsGamingSettings);
        Assert.False(viewModel.CanApplyWindowsGamingSettings);
        await viewModel.ApplyWindowsGamingSettingsAsync();
        Assert.Equal(0, registry.Read(GameModeRegistryAction.Address).NumericValue);

        installRelease.SetResult(SilentUpdateLaunch.Running());
        Assert.True(await updateTask);
        Assert.True(viewModel.CanApplyWindowsGamingSettings);
    }

    [Fact]
    public async Task RunningBenchmark_DisablesWindowsGamingUntilTheBenchmarkFinishes()
    {
        var benchmarkRelease = new TaskCompletionSource<AppGtaVBenchmarkResult>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        using var viewModel = new MainViewModel(
            new FakeAppOptimizationService(
                new AppSettings(),
                settingsFileExists: false,
                benchmarkResult: benchmarkRelease.Task),
            windowsGamingControls: CreateService(registry, new InMemoryJournalStore()));
        await viewModel.InitializeAsync();
        await viewModel.RefreshWindowsGamingSettingsAsync();

        var benchmarkTask = viewModel.RunGtaVBenchmarkAsync();

        Assert.True(viewModel.IsGtaVBenchmarkRunning);
        Assert.False(viewModel.CanRefreshWindowsGamingSettings);
        Assert.False(viewModel.CanApplyWindowsGamingSettings);
        await viewModel.ApplyWindowsGamingSettingsAsync();
        Assert.Equal(0, registry.Read(GameModeRegistryAction.Address).NumericValue);

        benchmarkRelease.SetResult(new AppGtaVBenchmarkResult
        {
            Succeeded = false,
            FailureReason = "gtav-not-detected",
            Iterations = []
        });
        await benchmarkTask;

        Assert.False(viewModel.IsGtaVBenchmarkRunning);
        Assert.True(viewModel.CanApplyWindowsGamingSettings);
    }

    [Fact]
    public async Task SystemPanel_ShowsFiveMBlockAndEnablesAfterARecheck()
    {
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(0));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            RegistryValueState.FromDword(1));
        var processInspector = new FakeProcessInspector(running: true);
        using var viewModel = new MainViewModel(
            new FakeAppOptimizationService(new AppSettings(), settingsFileExists: false),
            windowsGamingControls: CreateService(
                registry,
                new InMemoryJournalStore(),
                processInspector));
        await viewModel.InitializeAsync();

        await viewModel.RefreshWindowsGamingSettingsAsync();

        Assert.False(viewModel.CanApplyWindowsGamingSettings);
        Assert.Contains("FiveM", viewModel.WindowsGamingStatusMessage, StringComparison.Ordinal);

        processInspector.Running = false;
        await viewModel.RefreshWindowsGamingSettingsAsync();

        Assert.True(viewModel.CanApplyWindowsGamingSettings);
    }

    [Fact]
    public async Task WindowsGamingHistory_DoesNotReplaceLastOptimizationCard()
    {
        var record = new AppHistoryRecord
        {
            TransactionId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Profile = OptimizationProfile.Balanced,
            Kind = AppHistoryKind.WindowsGaming,
            State = "Applied",
            ChangedActions = 2,
            CanRollback = true
        };
        using var viewModel = new MainViewModel(new FakeAppOptimizationService(
            new AppSettings(),
            settingsFileExists: false,
            history: [record]));

        await viewModel.InitializeAsync();

        var historyItem = Assert.Single(viewModel.HistoryItems);
        Assert.Equal(AppHistoryKind.WindowsGaming, historyItem.Kind);
        Assert.False(viewModel.HasLastOptimization);
    }

    private sealed class MutatingOnThirdReadRegistryStore(
        FakeRegistryStore inner,
        Action<FakeRegistryStore> mutation) : IRegistryStore
    {
        private int readCount;

        public RegistryValueState Read(RegistryAddress address)
        {
            if (Interlocked.Increment(ref readCount) == 3)
            {
                mutation(inner);
            }

            return inner.Read(address);
        }

        public void Write(RegistryAddress address, RegistryValueState state) =>
            inner.Write(address, state);

        public void Delete(RegistryAddress address) => inner.Delete(address);
    }

    private sealed class BlockingJournalStore(Task release) : IWindowsTransactionJournalStore
    {
        private readonly Dictionary<Guid, WindowsTransactionJournal> journals = [];

        public TaskCompletionSource<bool> SaveStarted { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task SaveAsync(
            WindowsTransactionJournal journal,
            CancellationToken cancellationToken)
        {
            SaveStarted.TrySetResult(true);
            await release.WaitAsync(cancellationToken);
            journals[journal.TransactionId] = journal;
        }

        public Task<WindowsTransactionJournal?> LoadAsync(
            Guid transactionId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            journals.TryGetValue(transactionId, out var journal);
            return Task.FromResult(journal);
        }
    }

    private sealed class ThrowOnSecondLoadJournalStore : IWindowsTransactionJournalStore
    {
        private readonly InMemoryJournalStore inner = new();

        public int LoadCount { get; private set; }

        public Task SaveAsync(
            WindowsTransactionJournal journal,
            CancellationToken cancellationToken) => inner.SaveAsync(journal, cancellationToken);

        public Task<WindowsTransactionJournal?> LoadAsync(
            Guid transactionId,
            CancellationToken cancellationToken)
        {
            LoadCount++;
            if (LoadCount > 1)
            {
                throw new IOException("Unexpected journal reload after execution.");
            }

            return inner.LoadAsync(transactionId, cancellationToken);
        }
    }

    private sealed class FailAfterActionWriteJournalStore : IWindowsTransactionJournalStore
    {
        private bool persistentlyFailing;

        public Task SaveAsync(
            WindowsTransactionJournal journal,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (persistentlyFailing || journal.Actions.Any(entry =>
                    entry.State == ActionJournalState.Applied && entry.Changed))
            {
                persistentlyFailing = true;
                throw new IOException("Simulated persistent journal failure after a write.");
            }

            return Task.CompletedTask;
        }

        public Task<WindowsTransactionJournal?> LoadAsync(
            Guid transactionId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<WindowsTransactionJournal?>(null);
        }
    }

    private sealed class ThrowingProcessInspector : IFiveMProcessInspector
    {
        public bool IsAnyRunning() => throw new InvalidOperationException(
            "Simulated process inspection failure.");

        public bool IsRunningFrom(string installationRoot) => IsAnyRunning();
    }

    private sealed class SequencedProcessInspector(params bool[] sequence)
        : IFiveMProcessInspector
    {
        private readonly Queue<bool> states = new(sequence);
        private bool lastState;

        public bool IsAnyRunning()
        {
            if (states.Count > 0)
            {
                lastState = states.Dequeue();
            }

            return lastState;
        }

        public bool IsRunningFrom(string installationRoot) => IsAnyRunning();
    }

    private sealed class BlockingDownloadReleaseUpdateService(
        ReleaseUpdate update,
        Task<DownloadedUpdate> download) : IReleaseUpdateService
    {
        public TaskCompletionSource<bool> DownloadStarted { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<ReleaseUpdate?> CheckForUpdateAsync(
            StableSemanticVersion currentVersion,
            CancellationToken cancellationToken = default) => Task.FromResult<ReleaseUpdate?>(update);

        public Task<DownloadedUpdate> DownloadUpdateAsync(
            ReleaseUpdate requestedUpdate,
            IProgress<UpdateDownloadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            DownloadStarted.TrySetResult(true);
            return download;
        }
    }

    private sealed class BlockingSilentUpdateInstaller(Task<SilentUpdateLaunch> release)
        : ISilentUpdateInstaller
    {
        public TaskCompletionSource<bool> Started { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<SilentUpdateLaunch> StartAsync(
            DownloadedUpdate update,
            CancellationToken cancellationToken = default)
        {
            Started.TrySetResult(true);
            return release;
        }
    }

    private sealed class FailHistoricalCaptureWriteRegistryStore(IRegistryStore inner)
        : IRegistryStore
    {
        public RegistryValueState Read(RegistryAddress address) => inner.Read(address);

        public void Write(RegistryAddress address, RegistryValueState state)
        {
            if (address == GameDvrRegistryAction.HistoricalCaptureAddress)
            {
                throw new IOException("Simulated write failure.");
            }

            inner.Write(address, state);
        }

        public void Delete(RegistryAddress address) => inner.Delete(address);
    }
}
