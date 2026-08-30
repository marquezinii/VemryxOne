using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Ralven.App.Services;
using Ralven.App.ViewModels;
using Ralven.App.Views;
using UserControl = System.Windows.Controls.UserControl;

namespace Ralven.App.Views.Pages;

public partial class SystemPage : UserControl
{
    public SystemPage()
    {
        InitializeComponent();
    }

    private void OpenWindowsUpdate_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:windowsupdate");
    private void OpenSecurity_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:windowsdefender");
    private void OpenAbout_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:about");

    private async void SystemPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.RefreshWindowsGamingSettingsAsync();
        }
    }

    private async void RefreshWindowsGaming_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.RefreshWindowsGamingSettingsAsync();
        }
    }

    private async void ApplyWindowsGaming_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel || !viewModel.CanApplyWindowsGamingSettings)
        {
            return;
        }

        var localization = LocalizationService.Current;
        var dialog = new OptimizationConfirmationWindow(
            localization.GetString("System.Gaming.Confirm.Title"),
            localization.GetString("System.Gaming.Confirm.Message"),
            localization.GetString("System.Gaming.Confirm.Cancel"),
            localization.GetString("System.Gaming.Confirm.Apply"))
        {
            Owner = Window.GetWindow(this)
        };
        if (dialog.ShowDialog() == true)
        {
            await viewModel.ApplyWindowsGamingSettingsAsync();
        }
    }

    private async void RestoreWindowsGaming_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel || !viewModel.CanRestoreWindowsGamingSettings)
        {
            return;
        }

        var localization = LocalizationService.Current;
        var dialog = new OptimizationConfirmationWindow(
            localization.GetString("System.Gaming.RestoreConfirm.Title"),
            localization.GetString("System.Gaming.RestoreConfirm.Message"),
            localization.GetString("System.Gaming.Confirm.Cancel"),
            localization.GetString("System.Gaming.Restore"))
        {
            Owner = Window.GetWindow(this)
        };
        if (dialog.ShowDialog() == true)
        {
            await viewModel.RestoreWindowsGamingSettingsAsync();
        }
    }

    private static void OpenSettings(string uri) => ExternalLauncher.TryOpen(() => Process.Start(new ProcessStartInfo(uri)
    {
        UseShellExecute = true
    }));
}
