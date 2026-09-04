using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Ralven.App.Services;
using Ralven.App.ViewModels;
using Ralven.Windows.Infrastructure;
using UserControl = System.Windows.Controls.UserControl;

namespace Ralven.App.Views.Pages;

public partial class ApplicationsPage : UserControl, IDisposable
{
    private readonly ApplicationsPageViewModel viewModel;
    private readonly CancellationTokenSource lifetimeCancellation = new();
    private bool initialRefreshStarted;
    private bool disposed;

    public ApplicationsPage()
        : this(
            new WindowsApplicationInventoryInspector(),
            new WinGetApplicationUpdateService())
    {
    }

    internal ApplicationsPage(
        IWindowsApplicationInventoryInspector inspector,
        IWindowsApplicationUpdateService updateService)
    {
        InitializeComponent();
        viewModel = new ApplicationsPageViewModel(inspector, updateService);
        DataContext = viewModel;

        // Setting IsChecked in XAML fires Checked while named siblings are
        // still being created. Select the initial surface after the document
        // has been initialized instead.
        UpdatesTab.IsChecked = true;
        ShowInventory(ApplicationUpdatesPanel);
    }

    private async void ApplicationsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (initialRefreshStarted)
        {
            return;
        }

        initialRefreshStarted = true;
        await RefreshAllAsync();
    }

    private async void RefreshInventory_Click(object sender, RoutedEventArgs e)
    {
        await RefreshAllAsync();
    }

    private void InventoryTab_Checked(object sender, RoutedEventArgs e)
    {
        if (InstalledApplicationsPanel is null
            || StartupApplicationsPanel is null
            || ApplicationUpdatesPanel is null)
        {
            return;
        }

        ShowInventory(ReferenceEquals(sender, StartupTab)
            ? StartupApplicationsPanel
            : ReferenceEquals(sender, UpdatesTab)
                ? ApplicationUpdatesPanel
                : InstalledApplicationsPanel);
    }

    private void ShowInventory(UIElement selected)
    {
        InstalledApplicationsPanel.Visibility = ReferenceEquals(
            selected,
            InstalledApplicationsPanel)
            ? Visibility.Visible
            : Visibility.Collapsed;
        StartupApplicationsPanel.Visibility = ReferenceEquals(
            selected,
            StartupApplicationsPanel)
            ? Visibility.Visible
            : Visibility.Collapsed;
        ApplicationUpdatesPanel.Visibility = ReferenceEquals(
            selected,
            ApplicationUpdatesPanel)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private async Task RefreshAllAsync()
    {
        await Task.WhenAll(
            viewModel.RefreshAsync(lifetimeCancellation.Token),
            viewModel.CheckApplicationUpdatesAsync(lifetimeCancellation.Token));
    }

    private async void UpdateApplication_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button
            {
                Tag: ApplicationUpdateDisplayItem item
            })
        {
            return;
        }

        var localization = LocalizationService.Current;
        var dialog = new OptimizationConfirmationWindow(
            localization.GetString("Applications.Updates.Confirm.Title"),
            localization.Format(
                "Applications.Updates.Confirm.Message",
                item.Name,
                item.InstalledVersion,
                item.AvailableVersion,
                item.Source),
            localization.GetString("Applications.Updates.Confirm.Cancel"),
            localization.GetString("Applications.Updates.Confirm.Action"))
        {
            Owner = Window.GetWindow(this)
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        await viewModel.UpdateApplicationAsync(item, lifetimeCancellation.Token);
    }

    private void OpenInstalledApps_Click(object sender, RoutedEventArgs e) =>
        OpenSettings("ms-settings:appsfeatures");

    private void OpenStartupApps_Click(object sender, RoutedEventArgs e) =>
        OpenSettings("ms-settings:startupapps");

    private void OpenStoreUpdates_Click(object sender, RoutedEventArgs e) =>
        OpenSettings("ms-windows-store://downloadsandupdates");

    private void OpenDefaultApps_Click(object sender, RoutedEventArgs e) =>
        OpenSettings("ms-settings:defaultapps");

    private static void OpenSettings(string uri) => ExternalLauncher.TryOpen(
        () => Process.Start(new ProcessStartInfo(uri)
        {
            UseShellExecute = true
        }));

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        lifetimeCancellation.Cancel();
        lifetimeCancellation.Dispose();
        viewModel.Dispose();
    }
}
