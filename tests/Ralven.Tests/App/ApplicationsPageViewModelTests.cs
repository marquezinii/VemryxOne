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
        Assert.Equal(
            localization.GetString("Applications.Inventory.NoMatches.Startup"),
            viewModel.StartupEmptyMessage);
    }

    [Fact]
    public async Task RefreshAsync_DistinguishesEmptyAndIncompleteSubinventories()
    {
        var localization = new LocalizationService(CultureInfo.GetCultureInfo("en-US"));
        var snapshot = new WindowsApplicationInventorySnapshot(
            [],
            [],
            DateTimeOffset.UtcNow,
            InstalledApplicationsComplete: true,
            StartupItemsComplete: false);
        using var viewModel = new ApplicationsPageViewModel(
            new StubInventoryInspector(_ => Task.FromResult(snapshot)),
            localization);

        await viewModel.RefreshAsync(TestContext.Current.CancellationToken);

        Assert.True(viewModel.ShowInstalledEmptyState);
        Assert.True(viewModel.ShowStartupEmptyState);
        Assert.Equal(
            localization.GetString("Applications.Inventory.Empty.Installed"),
            viewModel.InstalledEmptyMessage);
        Assert.Equal(
            localization.GetString("Applications.Inventory.Incomplete.Startup"),
            viewModel.StartupEmptyMessage);
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
        Assert.True(viewModel.ShowInstalledEmptyState);
        Assert.True(viewModel.ShowStartupEmptyState);
        Assert.Equal(
            localization.GetString("Applications.Inventory.Unavailable.Installed"),
            viewModel.InstalledEmptyMessage);
        Assert.Equal(
            localization.GetString("Applications.Inventory.Unavailable.Startup"),
            viewModel.StartupEmptyMessage);
        Assert.True(viewModel.CanRefreshInventory);

        localization.SetLanguage(AppLanguage.English);
        Assert.Equal(
            localization.GetString("Applications.Inventory.Status.Unavailable"),
            viewModel.InventoryStatusMessage);
        Assert.Equal(
            localization.GetString("Applications.Inventory.Unavailable.Installed"),
            viewModel.InstalledEmptyMessage);
    }

    [Fact]
    public async Task RefreshAsync_FailureAfterSuccess_NotifiesUnavailableState()
    {
        var localization = new LocalizationService(CultureInfo.GetCultureInfo("en-US"));
        var attempts = 0;
        var snapshot = new WindowsApplicationInventorySnapshot(
            [],
            [],
            DateTimeOffset.UtcNow,
            InstalledApplicationsComplete: true,
            StartupItemsComplete: true);
        using var viewModel = new ApplicationsPageViewModel(
            new StubInventoryInspector(_ => ++attempts == 1
                ? Task.FromResult(snapshot)
                : Task.FromException<WindowsApplicationInventorySnapshot>(new IOException("refresh failed"))),
            localization);

        await viewModel.RefreshAsync(TestContext.Current.CancellationToken);
        var changedProperties = new HashSet<string>();
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is not null)
            {
                changedProperties.Add(args.PropertyName);
            }
        };

        await viewModel.RefreshAsync(TestContext.Current.CancellationToken);

        Assert.Empty(viewModel.InstalledApplications);
        Assert.Equal(
            localization.GetString("Applications.Inventory.Status.Unavailable"),
            viewModel.InventoryStatusMessage);
        Assert.Equal(
            localization.GetString("Applications.Inventory.Unavailable.Installed"),
            viewModel.InstalledEmptyMessage);
        Assert.Contains(nameof(ApplicationsPageViewModel.InstalledEmptyMessage), changedProperties);
        Assert.Contains(nameof(ApplicationsPageViewModel.StartupEmptyMessage), changedProperties);

        localization.SetLanguage(AppLanguage.PortugueseBrazil);
        Assert.Equal(
            localization.GetString("Applications.Inventory.Unavailable.Installed"),
            viewModel.InstalledEmptyMessage);
    }

    [Fact]
    public async Task ApplicationUpdates_AreFilteredAndRemovedAfterSuccessfulUpdate()
    {
        var localization = new LocalizationService(CultureInfo.GetCultureInfo("pt-BR"));
        var updates = new StubUpdateService(new WindowsApplicationUpdateSnapshot(
            [
                new("OBSProject.OBSStudio", "OBS Studio", "31.0", "32.1", "winget"),
                new("VideoLAN.VLC", "VLC media player", "3.0.21", "3.0.22", "winget")
            ],
            DateTimeOffset.UtcNow,
            IsWinGetAvailable: true));
        using var viewModel = new ApplicationsPageViewModel(
            new StubInventoryInspector(_ => Task.FromResult(new WindowsApplicationInventorySnapshot(
                [],
                [],
                DateTimeOffset.UtcNow,
                InstalledApplicationsComplete: true,
                StartupItemsComplete: true))),
            updates,
            localization);

        await viewModel.CheckApplicationUpdatesAsync(TestContext.Current.CancellationToken);
        viewModel.SearchText = "OBS";
        var item = Assert.Single(viewModel.ApplicationUpdates);
        await viewModel.UpdateApplicationAsync(item, TestContext.Current.CancellationToken);

        Assert.Equal("OBSProject.OBSStudio", updates.UpdatedPackageId);
        Assert.Equal(1, viewModel.ApplicationUpdateCount);
        Assert.Empty(viewModel.ApplicationUpdates);
        Assert.Equal(
            localization.Format("Applications.Updates.Status.Succeeded", "OBS Studio"),
            viewModel.ApplicationUpdateStatusMessage);
        Assert.True(viewModel.CanUpdateApplications);
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

    private sealed class StubUpdateService(WindowsApplicationUpdateSnapshot snapshot)
        : IWindowsApplicationUpdateService
    {
        public string? UpdatedPackageId { get; private set; }

        public Task<WindowsApplicationUpdateSnapshot> CheckAsync(
            CancellationToken cancellationToken = default) => Task.FromResult(snapshot);

        public Task<WindowsApplicationUpdateResult> UpdateAsync(
            WindowsApplicationUpdate update,
            CancellationToken cancellationToken = default)
        {
            UpdatedPackageId = update.PackageId;
            return Task.FromResult(new WindowsApplicationUpdateResult(
                WindowsApplicationUpdateOutcome.Succeeded,
                ExitCode: 0));
        }
    }
}
