using Ralven.App.Services;
using Ralven.App.ViewModels;
using Ralven.Contracts;
using Xunit;

namespace Ralven.Tests.App;

public sealed class MainViewModelEmptyPlanTests
{
    [Fact]
    public void BeforeDiagnostic_NoProfileIsPresentedAsRecommended()
    {
        var viewModel = new MainViewModel(new FakeAppOptimizationService(
            new AppSettings(),
            settingsFileExists: false));

        Assert.False(viewModel.IsLightRecommended);
        Assert.False(viewModel.IsBalancedRecommended);
        Assert.False(viewModel.IsAggressiveRecommended);
        Assert.False(viewModel.IsSelectedProfileRecommended);
    }

    [Theory]
    [InlineData(OptimizationProfile.Light)]
    [InlineData(OptimizationProfile.Balanced)]
    [InlineData(OptimizationProfile.Aggressive)]
    public async Task InitializeAsync_SelectsTheProfileRecommendedByHardware(
        OptimizationProfile recommendedProfile)
    {
        var viewModel = new MainViewModel(new FakeAppOptimizationService(
            new AppSettings(),
            settingsFileExists: false,
            recommendedProfile: recommendedProfile));

        await viewModel.InitializeAsync();

        Assert.Equal(recommendedProfile == OptimizationProfile.Light, viewModel.IsLightSelected);
        Assert.Equal(recommendedProfile == OptimizationProfile.Balanced, viewModel.IsBalancedSelected);
        Assert.Equal(recommendedProfile == OptimizationProfile.Aggressive, viewModel.IsAggressiveSelected);
        Assert.True(viewModel.IsSelectedProfileRecommended);
    }

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
        Assert.False(viewModel.CanStart);
    }

    [Fact]
    public async Task GeneralWindows_IsTheDefaultScopeAndDoesNotRequireFiveMInstallation()
    {
        var service = new FakeAppOptimizationService(
            new AppSettings(),
            settingsFileExists: false,
            edition: FiveMEdition.Unknown);
        var localization = new LocalizationService(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        var viewModel = new MainViewModel(service, localization);

        await viewModel.InitializeAsync();

        Assert.Equal(OptimizationScope.GeneralWindows, viewModel.OptimizationScope);
        Assert.Equal(localization.GetString("Optimizer.General.Title"), viewModel.OptimizerTitle);
        Assert.True(viewModel.CanStart);

        viewModel.SelectProfile(OptimizationProfile.Aggressive);

        Assert.Contains(
            localization.GetString("Plan.Notice.AggressiveWindows"),
            viewModel.PlanNoticesText,
            StringComparison.Ordinal);

        viewModel.SetOptimizationScope(OptimizationScope.FiveMLegacy);

        Assert.Equal(localization.GetString("Optimizer.FiveM.Title"), viewModel.OptimizerTitle);
        Assert.False(viewModel.CanStart);
    }

    [Fact]
    public async Task GeneralWindows_BlocksExecutionWhileFiveMIsRunning()
    {
        var viewModel = new MainViewModel(new FakeAppOptimizationService(
            new AppSettings(),
            settingsFileExists: false,
            isFiveMRunning: true));

        await viewModel.InitializeAsync();

        Assert.Equal(OptimizationScope.GeneralWindows, viewModel.OptimizationScope);
        Assert.False(viewModel.CanStart);
    }
}
