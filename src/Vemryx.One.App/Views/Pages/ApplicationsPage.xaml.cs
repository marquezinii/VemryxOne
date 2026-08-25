using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Vemryx.One.App.Services;
using UserControl = System.Windows.Controls.UserControl;

namespace Vemryx.One.App.Views.Pages;

public partial class ApplicationsPage : UserControl
{
    public ApplicationsPage()
    {
        InitializeComponent();
    }

    private void OpenInstalledApps_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:appsfeatures");
    private void OpenStartupApps_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:startupapps");
    private void OpenStoreUpdates_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-windows-store://downloadsandupdates");
    private void OpenDefaultApps_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:defaultapps");

    private static void OpenSettings(string uri) => ExternalLauncher.TryOpen(() => Process.Start(new ProcessStartInfo(uri)
    {
        UseShellExecute = true
    }));
}
