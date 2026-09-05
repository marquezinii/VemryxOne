using Ralven.Contracts;

namespace Ralven.Core.Planning;

public static class PersonalOptimizationPolicy
{
    public static OptimizationOptionsDto CreateOptions(PersonalOptimizationPreferencesDto preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        if (!Enum.IsDefined(preferences.Usage))
        {
            throw new ArgumentOutOfRangeException(nameof(preferences));
        }

        return new OptimizationOptionsDto
        {
            CleanUserTemporaryFiles = preferences.CleanOldTemporaryFiles,
            TemporaryFileMinimumAgeDays = 30,
            RemoveOldFiveMCrashDumps = false,
            EnableGameMode = preferences.Usage is PersonalUsage.Gaming or PersonalUsage.Streaming,
            PreferHighPerformanceGpu = false,
            DisableBackgroundCapture = !preferences.PreserveBackgroundCapture,
            UseSessionPerformancePowerPlan = preferences.AllowPerformancePower,
            ApplyLegacyGraphicsPreset = false,
            ReduceWindowsVisualEffects = !preferences.PreserveAppearance,
            ReduceMenuShowDelay = true,
            // ASPM changes both AC and battery settings. It does not belong to
            // a preference that only authorizes more power while plugged in.
            AdjustPciExpressPowerManagement = false
        };
    }
}
