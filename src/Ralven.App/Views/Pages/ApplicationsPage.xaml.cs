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
        : this(new WindowsApplicationInventoryInspector())
    {
    }

    internal ApplicationsPage(IWindowsApplicationInventoryInspector inspector)
    {
        InitializeComponent();
        viewModel = new ApplicationsPageViewModel(inspector);
        DataContext = viewModel;

        // Setting IsChecked in XAML fires Checked while named siblings are
        // still being created. Select the initial surface after the document
        // has been initialized instead.
        InstalledTab.IsChecked = true;
        ShowInventory(InstalledApplicationsPanel);
    }

    private async void ApplicationsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (initialRefreshStarted)
        {
            return;
        }

        initialRefreshStarted = true;
        await viewModel.RefreshAsync(lifetimeCancellation.Token);
    }

    private async void RefreshInventory_Click(object sender, RoutedEventArgs e)
    {
        await viewModel.RefreshAsync(lifetimeCancellation.Token);
    }

    private void InventoryTab_Checked(object sender, RoutedEventArgs e)
    {
        if (InstalledApplicationsPanel is null || StartupApplicationsPanel is null)
        {
            return;
        }

        ShowInventory(ReferenceEquals(sender, StartupTab)
            ? StartupApplicationsPanel
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
