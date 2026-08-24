using Vemryx.One.App.Services;
using Vemryx.One.App.ViewModels;
using Xunit;

namespace Vemryx.One.Tests.App;

public sealed class MainViewModelEmptyPlanTests
{
    [Fact]
    public async Task InitializeAsync_WhenDiagnosticFails_DoesNotClaimFiveMIsMissing()
    {
        var service = new FakeAppOptimizationService(
            new AppSettings(),
            settingsFileExists: false,
            diagnosticException: new IOException("diagnostic failed"));
        var localization = new LocalizationService(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        var viewModel = new MainViewModel(service, localization);

        await viewModel.InitializeAsync();

        Assert.Equal(
            localization.GetString("Plan.Empty.DiagnosticUnavailable"),
            viewModel.EmptyPlanMessage);
    }
}
