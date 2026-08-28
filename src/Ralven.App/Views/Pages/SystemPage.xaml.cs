using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Ralven.App.Services;
using UserControl = System.Windows.Controls.UserControl;

namespace Ralven.App.Views.Pages;

public partial class SystemPage : UserControl
{
    public SystemPage()
    {
        InitializeComponent();
    }

    private void OpenWindowsUpdate_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:windowsupdate");
    private void OpenStorage_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:storagesense");
    private void OpenSecurity_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:windowsdefender");
    private void OpenAbout_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:about");

    private static void OpenSettings(string uri) => ExternalLauncher.TryOpen(() => Process.Start(new ProcessStartInfo(uri)
    {
        UseShellExecute = true
    }));
}
