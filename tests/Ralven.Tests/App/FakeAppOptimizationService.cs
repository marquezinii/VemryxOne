using Ralven.App.Services;
using Ralven.Contracts;

namespace Ralven.Tests.App;

/// <summary>
/// Minimal, in-memory double of <see cref="IAppOptimizationService"/> used
/// only to exercise <see cref="Ralven.App.ViewModels.MainViewModel"/>'s
/// privacy consent wiring (loading settings, computing the consent decision,
/// and persisting the outcome) without touching real disk. Methods the
/// consent flow does not call throw <see cref="NotSupportedException"/> so a
/// test that accidentally depends on unrelated behavior fails loudly instead
/// of silently returning a meaningless default.
/// </summary>
public sealed class FakeAppOptimizationService : IAppOptimizationService
{
    private readonly bool settingsFileExists;
    private readonly IReadOnlyList<AppHistoryRecord> history;
    private readonly bool isFiveMRunning;
    private readonly Task<AppGtaVBenchmarkResult>? benchmarkResult;
    private readonly bool? rollbackResult;
    private readonly AppProgressUpdate? rollbackProgressUpdate;
    public AppSettings? SavedSettings { get; private set; }
    public int SaveCallCount { get; private set; }

    public FakeAppOptimizationService(
        AppSettings initialSettings,
        bool settingsFileExists,
        Exception? diagnosticException = null,
        IReadOnlyList<AppHistoryRecord>? history = null,
        bool isFiveMRunning = false,
        Task<AppGtaVBenchmarkResult>? benchmarkResult = null,
        bool? rollbackResult = null,
        AppProgressUpdate? rollbackProgressUpdate = null)
    {
        InitialSettings = initialSettings;
        this.settingsFileExists = settingsFileExists;
        DiagnosticException = diagnosticException;
        this.history = history ?? [];
        this.isFiveMRunning = isFiveMRunning;
        this.benchmarkResult = benchmarkResult;
        this.rollbackResult = rollbackResult;
        this.rollbackProgressUpdate = rollbackProgressUpdate;
    }

    public AppSettings InitialSettings { get; }

    public Exception? DiagnosticException { get; }

    public string LogsDirectory => throw new NotSupportedException();

    public bool SettingsFileExists() => settingsFileExists;

    public Task<AppSettings> LoadSettingsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(InitialSettings);

    public Task SaveSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        SavedSettings = settings;
        SaveCallCount++;
        return Task.CompletedTask;
    }

    public Task<AppDiagnostic> DiagnoseAsync(CancellationToken cancellationToken = default) =>
        DiagnosticException is null
            ? Task.FromResult(CreateMinimalDiagnostic(isFiveMRunning))
            : Task.FromException<AppDiagnostic>(DiagnosticException);

    public Task<IReadOnlyList<AppHistoryRecord>> LoadHistoryAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(history);

    public Task<AppOptimizationResult> ExecuteAsync(
        OptimizationPlanDto plan,
        IProgress<AppProgressUpdate> progress,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<bool> RollbackAsync(
        Guid transactionId,
        IProgress<AppProgressUpdate> progress,
        CancellationToken cancellationToken = default)
    {
        if (rollbackResult is null)
        {
            throw new NotSupportedException();
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (rollbackProgressUpdate is not null)
        {
            progress.Report(rollbackProgressUpdate);
        }

        return Task.FromResult(rollbackResult.Value);
    }

    public Task<AppGtaVBenchmarkResult> RunGtaVBenchmarkAsync(
        int iterations,
        CancellationToken cancellationToken = default) =>
        benchmarkResult ?? throw new NotSupportedException();

    private static AppDiagnostic CreateMinimalDiagnostic(bool isFiveMRunning) => new()
    {
        Edition = FiveMEdition.Legacy,
        IsFiveMRunning = isFiveMRunning,
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
    };
}

/// <summary>
/// Records whether telemetry was enabled/disabled and whether any event was
/// ever tracked, so tests can assert that confirming (or declining) privacy
/// consent never sends anything by itself.
/// </summary>
public sealed class RecordingTelemetryService : IAnonymousTelemetryService
{
    public bool IsEnabled { get; private set; }

    public int TrackCallCount { get; private set; }

    public void SetEnabled(bool enabled) => IsEnabled = enabled;

    public Task TrackAsync(AnonymousTelemetryEvent telemetryEvent, CancellationToken cancellationToken = default)
    {
        TrackCallCount++;
        return Task.CompletedTask;
    }

    public long SuccessfulSends => 0;
    public long FailedSends => 0;
    public bool IsHealthy => true;
}
