using Xunit;

namespace Ralven.Tests.App;

public sealed class OptimizationInterruptionUiTests
{
    [Fact]
    public void MainWindow_ConfirmsBeforeCancellingOrClosingAnActiveOptimization()
    {
        var source = TestHelpers.ReadMainWindowSource();

        Assert.Contains("ConfirmOptimizationInterruption(closeApplication: false)", source, StringComparison.Ordinal);
        Assert.Contains("ConfirmOptimizationInterruption(closeApplication: true)", source, StringComparison.Ordinal);
        Assert.Contains("closeAfterOptimizationStops = true", source, StringComparison.Ordinal);
        Assert.DoesNotContain("CancelOptimization_Click(object sender, RoutedEventArgs e) =>", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainWindow_BlocksCloseAndKeepsTrayAccessDuringWindowsGamingMutation()
    {
        var root = FindRepositoryRoot();
        var traySource = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Ralven.App",
            "MainWindow.Tray.xaml.cs"));
        var viewModelSource = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Ralven.App",
            "ViewModels",
            "MainViewModel.System.cs"));

        Assert.Contains(
            "if (viewModel.IsWindowsGamingBusy && !systemSessionEnding)",
            traySource,
            StringComparison.Ordinal);
        Assert.Contains("public bool IsWindowsGamingBusy", viewModelSource, StringComparison.Ordinal);

        var exitGuard = traySource.LastIndexOf(
            "if (viewModel.IsWindowsGamingBusy)",
            StringComparison.Ordinal);
        Assert.True(exitGuard >= 0);
        var reactivate = traySource.IndexOf("RequestActivation();", exitGuard, StringComparison.Ordinal);
        var hideTray = traySource.IndexOf("trayIcon.Hide();", reactivate, StringComparison.Ordinal);
        Assert.True(reactivate > exitGuard && hideTray > reactivate);
    }

    [Fact]
    public void OptimizerPlan_ShowsUserFacingRiskAndPrivilegeChips()
    {
        var root = FindRepositoryRoot();
        var source = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Ralven.App",
            "Views",
            "Pages",
            "OptimizerPage.xaml"));

        Assert.Contains("Text=\"{Binding Name}\"", source, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding Description}\"", source, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding RiskLabel}\"", source, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding PrivilegeLabel}\"", source, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding PlanHeader}\"", source, StringComparison.Ordinal);
        Assert.Contains("HasPlannedActions", source, StringComparison.Ordinal);
        Assert.Contains("EmptyPlanMessage", source, StringComparison.Ordinal);
        Assert.Contains("[Plan.Empty.Title], Source={StaticResource LocalizedStrings}, Mode=OneWay", source, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding EmptyPlanMessage, Mode=OneWay}\"", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainNavigation_ExposesGeneralOptimizerAndKeepsFiveMUnderGames()
    {
        var root = FindRepositoryRoot();
        var mainWindow = File.ReadAllText(Path.Combine(root, "src", "Ralven.App", "MainWindow.xaml"));
        var navigation = File.ReadAllText(Path.Combine(root, "src", "Ralven.App", "MainWindow.Navigation.xaml.cs"));
        var games = File.ReadAllText(Path.Combine(root, "src", "Ralven.App", "Views", "Pages", "GamesPage.xaml.cs"));
        var capture = File.ReadAllText(Path.Combine(root, "src", "Ralven.App", "MainWindow.Capture.xaml.cs"));

        Assert.Contains("x:Name=\"OptimizerNav\"", mainWindow, StringComparison.Ordinal);
        Assert.Contains("Tag=\"Optimizer\"", mainWindow, StringComparison.Ordinal);
        Assert.Contains("RequestNavigateToOptimizer(OptimizationScope.GeneralWindows)", navigation, StringComparison.Ordinal);
        Assert.Contains("RequestNavigateToOptimizer(OptimizationScope.FiveMLegacy)", games, StringComparison.Ordinal);
        Assert.Contains("\"FiveMOptimizer\"", capture, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ralven.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Raiz do repositório não encontrada.");
    }
}
