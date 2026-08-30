using Microsoft.Win32;
using Ralven.Contracts;
using Ralven.Windows.Actions;

namespace Ralven.Windows.Infrastructure;

public sealed class WindowsGamingSettingsInspector
{
    private readonly IRegistryStore registry;

    public WindowsGamingSettingsInspector(IRegistryStore registry)
    {
        this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public WindowsGamingSettingsDto Inspect()
    {
        return new WindowsGamingSettingsDto(
            Read(GameModeRegistryAction.Address),
            Read(GameDvrRegistryAction.HistoricalCaptureAddress));
    }

    private WindowsGamingSettingState Read(RegistryAddress address)
    {
        try
        {
            var value = registry.Read(address);
            if (!value.Exists)
            {
                return WindowsGamingSettingState.NotConfigured;
            }

            if (value.Kind != RegistryValueKind.DWord || value.NumericValue is not (0 or 1))
            {
                return WindowsGamingSettingState.Unavailable;
            }

            return value.NumericValue == 1
                ? WindowsGamingSettingState.Enabled
                : WindowsGamingSettingState.Disabled;
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException
            or System.Security.SecurityException
            or NotSupportedException)
        {
            return WindowsGamingSettingState.Unavailable;
        }
    }
}
