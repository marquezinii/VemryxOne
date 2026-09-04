using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.Windows;

public sealed class WinGetApplicationUpdateServiceTests
{
    private const string WinGetPath =
        @"C:\Users\tester\AppData\Local\Microsoft\WindowsApps\winget.exe";

    [Fact]
    public async Task CheckAsync_ParsesTheFixedWidthTableAndUsesReadOnlyArguments()
    {
        var runner = new RecordingCommandRunner(new CommandResult(
            0,
            """
            Nome                 ID                    Versão  Disponível
            --------------------------------------------------------------
            OBS Studio           OBSProject.OBSStudio  31.0    32.1.2
            Linha inválida       pacote…               1.0     2.0
            """,
            string.Empty));
        var service = new WinGetApplicationUpdateService(runner, WinGetPath);

        var snapshot = await service.CheckAsync(TestContext.Current.CancellationToken);

        Assert.True(snapshot.IsWinGetAvailable);
        var update = Assert.Single(snapshot.Updates);
        Assert.Equal("OBSProject.OBSStudio", update.PackageId);
        Assert.Equal("31.0", update.InstalledVersion);
        Assert.Equal("32.1.2", update.AvailableVersion);
        Assert.Equal(
            [
                "list",
                "--upgrade-available",
                "--source",
                "winget",
                "--sort",
                "name",
                "--ascending",
                "--disable-interactivity"
            ],
            runner.Arguments);
    }

    [Fact]
    public async Task UpdateAsync_UsesAnExactValidatedIdAndKeepsSecurityChecksEnabled()
    {
        var runner = new RecordingCommandRunner(new CommandResult(0, string.Empty, string.Empty));
        var service = new WinGetApplicationUpdateService(runner, WinGetPath);
        var update = new WindowsApplicationUpdate(
            "VideoLAN.VLC",
            "VLC media player",
            "3.0.21",
            "3.0.22",
            "winget");

        var result = await service.UpdateAsync(
            update,
            TestContext.Current.CancellationToken);

        Assert.Equal(WindowsApplicationUpdateOutcome.Succeeded, result.Outcome);
        Assert.Equal(
            [
                "upgrade",
                "--id",
                "VideoLAN.VLC",
                "--exact",
                "--source",
                "winget",
                "--accept-package-agreements",
                "--accept-source-agreements",
                "--disable-interactivity"
            ],
            runner.Arguments);
        Assert.DoesNotContain("--ignore-security-hash", runner.Arguments);
        Assert.DoesNotContain("--allow-reboot", runner.Arguments);
    }

    [Fact]
    public void ParseUpdates_WhenSuccessfulOutputHasNoTable_ReturnsAnEmptyList()
    {
        var updates = WinGetApplicationUpdateService.ParseUpdates(
            "Nenhum pacote instalado foi encontrado que corresponda aos critérios de entrada.");

        Assert.Empty(updates);
    }

    private sealed class RecordingCommandRunner(CommandResult result) : ICommandRunner
    {
        public IReadOnlyList<string> Arguments { get; private set; } = [];

        public Task<CommandResult> RunAsync(
            string executable,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            Assert.Equal(WinGetPath, executable);
            Arguments = arguments;
            return Task.FromResult(result);
        }
    }
}
