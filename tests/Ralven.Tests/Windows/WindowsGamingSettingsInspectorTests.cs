using Microsoft.Win32;
using Ralven.Contracts;
using Ralven.Windows.Actions;
using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.Windows;

public sealed class WindowsGamingSettingsInspectorTests
{
    [Fact]
    public void RegistryValueNameLookup_MatchesWindowsCaseInsensitively()
    {
        Assert.True(WindowsRegistryStore.ContainsValueName(
            ["autogamemodeenabled"],
            "AutoGameModeEnabled"));
    }

    [Fact]
    public void Inspect_ReportsConfiguredAndMissingStatesWithoutGuessing()
    {
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromDword(1));

        var result = new WindowsGamingSettingsInspector(registry).Inspect();

        Assert.Equal(WindowsGamingSettingState.Enabled, result.GameMode);
        Assert.Equal(WindowsGamingSettingState.NotConfigured, result.BackgroundCapture);
    }

    [Fact]
    public void Inspect_RejectsUnexpectedKindsAndValuesAsUnavailable()
    {
        var registry = new FakeRegistryStore();
        registry.Write(GameModeRegistryAction.Address, RegistryValueState.FromString("1"));
        registry.Write(
            GameDvrRegistryAction.HistoricalCaptureAddress,
            new RegistryValueState
            {
                Exists = true,
                Kind = RegistryValueKind.DWord,
                NumericValue = 7
            });

        var result = new WindowsGamingSettingsInspector(registry).Inspect();

        Assert.Equal(WindowsGamingSettingState.Unavailable, result.GameMode);
        Assert.Equal(WindowsGamingSettingState.Unavailable, result.BackgroundCapture);
    }
}
