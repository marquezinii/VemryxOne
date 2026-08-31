using System.Globalization;
using Ralven.App.Services;
using Ralven.App.ViewModels;
using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.App;

public sealed class ApplicationsPageViewModelTests
{
    [Fact]
    public async Task RefreshAsync_PresentsPartialInventoryAndFiltersInstalledApplications()
    {
        var localization = new LocalizationService(CultureInfo.GetCultureInfo("en-US"));
        var snapshot = new WindowsApplicationInventorySnapshot(
            [
                new("Alpha", "1.0", "Vendor A", 512L * 1024 * 1024,
                    WindowsApplicationScope.CurrentUser, WindowsApplicationArchitecture.X64),
                new("Beta", "2.0", "Vendor B", 1536L * 1024 * 1024,
                    WindowsApplicationScope.LocalMachine, WindowsApplicationArchitecture.X64)
            ],
            [
                new("Ralven", "CurrentUser:RegistryRun", WindowsStartupItemSource.RegistryRun,
                    WindowsApplicationScope.CurrentUser)
            ],
            new DateTimeOffset(2026, 8, 30, 12, 0, 0, TimeSpan.Zero),
            InstalledApplicationsComplete: false,
            StartupItemsComplete: true);
        using var viewModel = new ApplicationsPageViewModel(
            new StubInventoryInspector(_ => Task.FromResult(snapshot)),
            localization);

        await viewModel.RefreshAsync(TestContext.Current.CancellationToken);
        viewModel.SearchText = "Vendor B";

        Assert.Equal(2, viewModel.InstalledApplicationCount);
        Assert.Equal(1, viewModel.StartupItemCount);
        Assert.Equal(
            localization.GetString("Applications.Inventory.Status.Partial"),
            viewModel.InventoryStatusMessage);
        var application = Assert.Single(viewModel.InstalledApplications);
        Assert.Equal("Beta", application.Name);
        Assert.Equal("1.5 GB", application.EstimatedSize);
        Assert.Empty(viewModel.StartupItems);
    }

    [Fact]
    public async Task RefreshAsync_WhenInspectionFails_ShowsUnavailableState()
    {
        var localization = new LocalizationService(CultureInfo.GetCultureInfo("pt-BR"));
        using var viewModel = new ApplicationsPageViewModel(
            new StubInventoryInspector(_ => Task.FromException<WindowsApplicationInventorySnapshot>(
                new IOException("test failure"))),
            localization);

        await viewModel.RefreshAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            localization.GetString("Applications.Inventory.Status.Unavailable"),
            viewModel.InventoryStatusMessage);
        Assert.Empty(viewModel.InstalledApplications);
        Assert.Empty(viewModel.StartupItems);
        Assert.True(viewModel.CanRefreshInventory);
    }

    private sealed class StubInventoryInspector(
        Func<CancellationToken, Task<WindowsApplicationInventorySnapshot>> inspect)
        : IWindowsApplicationInventoryInspector
    {
        public Task<WindowsApplicationInventorySnapshot> InspectAsync(
            CancellationToken cancellationToken = default) => inspect(cancellationToken);

        public Task<WindowsApplicationInventorySnapshot> InspectStartupAsync(
            CancellationToken cancellationToken = default) => inspect(cancellationToken);
    }
}
