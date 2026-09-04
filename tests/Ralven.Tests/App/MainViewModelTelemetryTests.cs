using Ralven.App.Services;
using Ralven.App.ViewModels;
using Ralven.Contracts;
using Xunit;

namespace Ralven.Tests.App;

/// <summary>
/// Guards the anonymous telemetry contract at the
/// <see cref="MainViewModel"/> boundary: every event produced by an
/// optimization run must be small enough for
/// <see cref="TelemetryEventValidator"/> to accept, even when the underlying
/// plan legitimately contains far more actions than the wire format allows.
/// </summary>
public sealed class MainViewModelTelemetryTests
{
    [Fact]
    public async Task StartOptimizationAsync_LargePlan_TruncatesTelemetryActionIdsToTheValidatorLimit()
    {
        var service = new SucceedingAppOptimizationService();
        var telemetry = new CapturingTelemetryService();
        var viewModel = new MainViewModel(service, telemetry: telemetry);
        await viewModel.InitializeAsync();
        viewModel.SetOptimizationScope(OptimizationScope.FiveMLegacy);

        // The real catalog produces an executable plan with more actions than
        // the telemetry wire format allows; that is what used to make the
        // validator reject the event and silently drop it.
        Assert.True(viewModel.PlannedActions.Count > TelemetryEventValidator.MaxActionIds);
        Assert.NotEmpty(viewModel.PlannedAdjustments);
        Assert.NotEmpty(viewModel.InformationalPlannedActions);
        Assert.Equal(
            viewModel.PlannedActions.Count,
            viewModel.PlannedAdjustments.Count + viewModel.InformationalPlannedActions.Count);
        Assert.Equal(viewModel.PlannedAdjustments.Count, viewModel.SelectedActionCount);

        await viewModel.StartOptimizationAsync();

        var sent = await telemetry.WaitForEventAsync().WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
        Assert.Equal(TelemetryEventValidator.MaxActionIds, sent.ActionIds?.Count);
        TelemetryEventValidator.Validate(sent);
    }

    /// <summary>
    /// A broker/elevation-phase failure happens before any action reaches the
    /// journal, so there is no report line to read a BugCode from -- the run
    /// must fall back to <see cref="AppOptimizationResult.BugCode"/> instead
    /// of silently showing/telemetering no code at all.
    /// </summary>
    [Fact]
    public async Task StartOptimizationAsync_BrokerFailureWithNoReportLines_FallsBackToResultBugCode()
    {
        var service = new BrokerFailureAppOptimizationService();
        var telemetry = new CapturingTelemetryService();
        var viewModel = new MainViewModel(service, telemetry: telemetry);
        await viewModel.InitializeAsync();
        viewModel.SetOptimizationScope(OptimizationScope.FiveMLegacy);

        await viewModel.StartOptimizationAsync();

        var sent = await telemetry.WaitForEventAsync().WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
        Assert.Equal(BugCode.BRK_ACTION_EXECUTION, sent.BugCode);
        Assert.Contains("BRK_ACTION_EXECUTION", viewModel.ProgressHeadline);
    }

    /// <summary>Fails before any action runs (no journal/report), carrying
    /// only a run-level BugCode -- exactly the broker-phase failure shape.</summary>
    private sealed class BrokerFailureAppOptimizationService : IAppOptimizationService
    {
        public string LogsDirectory => throw new NotSupportedException();

        public bool SettingsFileExists() => true;

        public Task<AppSettings> LoadSettingsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new AppSettings
            {
                ShareAnonymousTelemetry = true,
                ShareCrashReports = true,
                PrivacyConsentVersion = PrivacyConsentPolicy.CurrentVersion
            });

        public Task SaveSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<AppDiagnostic> DiagnoseAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new AppDiagnostic
            {
                Edition = FiveMEdition.Legacy,
                IsFiveMRunning = false,
                GtaVDetected = false,
                GtaVIsRunning = false,
                GtaVGraphicsSettingsPath = string.Empty,
                CpuName = "Test CPU",
                GpuName = "Test GPU",
                TotalMemoryGiB = 16,
                AvailableMemoryGiB = 8,
                LogicalProcessorCount = 8,
                FreeDiskGiB = 100,
                LegacyCacheBytes = 0,
                OsLabel = "Windows 11",
                ReadinessScore = 80,
                RecommendedProfile = OptimizationProfile.Balanced,
                PerformancePressure = PerformancePressureLevel.Low,
                StreamingSoftware = new StreamingSoftwareSnapshot([], DateTimeOffset.UtcNow, true, true)
            });

        public Task<IReadOnlyList<AppHistoryRecord>> LoadHistoryAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AppHistoryRecord>>([]);

        public Task<AppOptimizationResult> ExecuteAsync(
            OptimizationPlanDto plan,
            IProgress<AppProgressUpdate> progress,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AppOptimizationResult
            {
                TransactionId = Guid.NewGuid(),
                Succeeded = false,
                WasCancelled = false,
                Summary = "O componente administrativo não confirmou o resultado.",
                CompletedActions = 0,
                BytesFreed = 0,
                Report = null,
                BugCode = BugCode.BRK_ACTION_EXECUTION
            });

        public Task<bool> RollbackAsync(
            Guid transactionId,
            IProgress<AppProgressUpdate> progress,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AppGtaVBenchmarkResult> RunGtaVBenchmarkAsync(
            int iterations,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    /// <summary>Runs a plan to successful completion instead of throwing, so
    /// the whole <see cref="MainViewModel.StartOptimizationAsync"/> telemetry
    /// path (including the <c>finally</c> block) is exercised.</summary>
    private sealed class SucceedingAppOptimizationService : IAppOptimizationService
    {
        public string LogsDirectory => throw new NotSupportedException();

        public bool SettingsFileExists() => true;

        public Task<AppSettings> LoadSettingsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new AppSettings
            {
                ShareAnonymousTelemetry = true,
                ShareCrashReports = true,
                PrivacyConsentVersion = PrivacyConsentPolicy.CurrentVersion
            });

        public Task SaveSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<AppDiagnostic> DiagnoseAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new AppDiagnostic
            {
                Edition = FiveMEdition.Legacy,
                IsFiveMRunning = false,
                GtaVDetected = false,
                GtaVIsRunning = false,
                GtaVGraphicsSettingsPath = string.Empty,
                CpuName = "Test CPU",
                GpuName = "Test GPU",
                TotalMemoryGiB = 16,
                AvailableMemoryGiB = 8,
                LogicalProcessorCount = 8,
                FreeDiskGiB = 100,
                LegacyCacheBytes = 0,
                OsLabel = "Windows 11",
                ReadinessScore = 80,
                RecommendedProfile = OptimizationProfile.Balanced,
                PerformancePressure = PerformancePressureLevel.Low,
                StreamingSoftware = new StreamingSoftwareSnapshot([], DateTimeOffset.UtcNow, true, true)
            });

        public Task<IReadOnlyList<AppHistoryRecord>> LoadHistoryAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AppHistoryRecord>>([]);

        public Task<AppOptimizationResult> ExecuteAsync(
            OptimizationPlanDto plan,
            IProgress<AppProgressUpdate> progress,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AppOptimizationResult
            {
                TransactionId = Guid.NewGuid(),
                Succeeded = true,
                WasCancelled = false,
                Summary = "done",
                CompletedActions = plan.Actions.Count,
                BytesFreed = 0
            });

        public Task<bool> RollbackAsync(
            Guid transactionId,
            IProgress<AppProgressUpdate> progress,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AppGtaVBenchmarkResult> RunGtaVBenchmarkAsync(
            int iterations,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class CapturingTelemetryService : IAnonymousTelemetryService
    {
        private readonly TaskCompletionSource<AnonymousTelemetryEvent> captured =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool IsEnabled { get; private set; }

        public bool IncludesOptionalData { get; private set; }

        public void Configure(bool enabled, bool includeOptionalData)
        {
            IsEnabled = enabled;
            IncludesOptionalData = includeOptionalData;
        }

        public Task<AnonymousTelemetryEvent> WaitForEventAsync() => captured.Task;

        public Task TrackAsync(AnonymousTelemetryEvent telemetryEvent, CancellationToken cancellationToken = default)
        {
            captured.TrySetResult(telemetryEvent);
            return Task.CompletedTask;
        }

        public long SuccessfulSends => 0;
        public long FailedSends => 0;
        public bool IsHealthy => true;
    }
}
