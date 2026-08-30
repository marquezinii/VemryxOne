namespace Ralven.Contracts;

public enum WindowsGamingSettingState
{
    Unknown = 0,
    Enabled = 1,
    Disabled = 2,
    NotConfigured = 3,
    Unavailable = 4
}

public sealed record WindowsGamingSettingsDto(
    WindowsGamingSettingState GameMode,
    WindowsGamingSettingState BackgroundCapture);
