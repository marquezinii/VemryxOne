using System.Globalization;
using Ralven.App.Services;
using Ralven.App.ViewModels;
using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.App;

public sealed class AppOptimizationServiceRollbackTests
{
    [Fact]
    public void HandleRollbackFailure_WhenFiveMIsRunning_ReportsSpecificLocalizedGuidance()
    {
        var localization = CreatePortugueseLocalization();
        var updates = new List<AppProgressUpdate>();

        var restored = AppOptimizationService.HandleRollbackFailure(
            new StubProcessInspector(running: true),
            localization,
            new InlineProgress<AppProgressUpdate>(updates.Add));

        Assert.False(restored);
        var update = Assert.Single(updates);
        Assert.Equal(AppProgressKind.Warning, update.Kind);
        Assert.Equal(100, update.Percent);
        Assert.Equal(localization.GetString("Runtime.RestoreBlockedFiveM"), update.Headline);
        Assert.Equal(localization.GetString("Runtime.RestoreBlockedFiveM"), update.Detail);
    }

    [Fact]
    public void HandleRollbackFailure_WhenProcessInspectionFails_ReportsFailClosedGuidance()
    {
        var localization = CreatePortugueseLocalization();
        var updates = new List<AppProgressUpdate>();

        var restored = AppOptimizationService.HandleRollbackFailure(
            new StubProcessInspector(exception: new InvalidOperationException("inspection unavailable")),
            localization,
            new InlineProgress<AppProgressUpdate>(updates.Add));

        Assert.False(restored);
        var update = Assert.Single(updates);
        Assert.Equal(AppProgressKind.Warning, update.Kind);
        Assert.Equal(localization.GetString("Runtime.RestoreProcessCheckFailed"), update.Headline);
        Assert.Equal(localization.GetString("Runtime.RestoreProcessCheckFailed"), update.Detail);
    }

    [Fact]
    public void HandleRollbackFailure_WhenNoProcessBlockExists_PreservesGenericConflict()
    {
        var localization = CreatePortugueseLocalization();
        var updates = new List<AppProgressUpdate>();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            AppOptimizationService.HandleRollbackFailure(
                new StubProcessInspector(running: false),
                localization,
                new InlineProgress<AppProgressUpdate>(updates.Add)));

        Assert.Equal(localization.GetString("Runtime.RollbackConflict"), exception.Message);
        Assert.Empty(updates);
    }

    [Theory]
    [InlineData("Runtime.RestoreBlockedFiveM")]
    [InlineData("Runtime.RestoreProcessCheckFailed")]
    public async Task RollbackAsync_WhenServiceReportsSpecificWarning_ShowsItInMainViewModelHeadline(
        string messageKey)
    {
        var localization = CreatePortugueseLocalization();
        var message = localization.GetString(messageKey);
        var service = new FakeAppOptimizationService(
            new AppSettings(),
            settingsFileExists: false,
            rollbackResult: false,
            rollbackProgressUpdate: new AppProgressUpdate
            {
                Timestamp = DateTimeOffset.UtcNow,
                Kind = AppProgressKind.Warning,
                Percent = 100,
                Headline = message,
                Detail = message
            });
        using var viewModel = new MainViewModel(service, localization);
        var item = new HistoryDisplayItem(
            Guid.NewGuid(),
            "Windows Gaming",
            "today",
            "two settings",
            CanRollback: true,
            AppHistoryKind.WindowsGaming);
        var previousContext = SynchronizationContext.Current;

        try
        {
            SynchronizationContext.SetSynchronizationContext(new InlineSynchronizationContext());

            var restored = await viewModel.RollbackAsync(item);

            Assert.False(restored);
            Assert.Equal(message, viewModel.ProgressHeadline);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }

    private static LocalizationService CreatePortugueseLocalization()
    {
        return new LocalizationService(CultureInfo.GetCultureInfo("pt-BR"));
    }

    private sealed class StubProcessInspector(
        bool running = false,
        Exception? exception = null) : IFiveMProcessInspector
    {
        public bool IsAnyRunning()
        {
            if (exception is not null)
            {
                throw exception;
            }

            return running;
        }

        public bool IsRunningFrom(string installationRoot) => IsAnyRunning();
    }

    private sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    private sealed class InlineSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback callback, object? state) => callback(state);
    }
}
