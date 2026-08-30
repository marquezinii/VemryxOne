using Ralven.Contracts;

namespace Ralven.App.Services;

internal sealed record HardwareProfileAssessment(
    int ReadinessScore,
    OptimizationProfile RecommendedProfile,
    PerformancePressureLevel PerformancePressure);

/// <summary>
/// Produces a transparent capacity assessment from values read locally. This
/// is not an FPS benchmark and deliberately does not claim a numeric gain.
/// </summary>
internal static class HardwareProfileAdvisor
{
    public static HardwareProfileAssessment Assess(
        double totalMemoryGiB,
        double availableMemoryGiB,
        int logicalProcessorCount,
        double freeDiskGiB,
        bool gpuWasIdentified)
    {
        var processors = Math.Max(1, logicalProcessorCount);
        var score = totalMemoryGiB >= 16 ? 25 : totalMemoryGiB >= 8 ? 15 : 7;
        score += availableMemoryGiB >= 8 ? 15 : availableMemoryGiB >= 4 ? 10 : availableMemoryGiB >= 2 ? 6 : 2;
        score += processors >= 12 ? 20 : processors >= 8 ? 16 : processors >= 4 ? 10 : 5;
        score += freeDiskGiB >= 30 ? 20 : freeDiskGiB >= 15 ? 13 : freeDiskGiB >= 8 ? 7 : 3;
        score += gpuWasIdentified ? 20 : 5;
        score = Math.Clamp(score, 0, 100);

        var pressurePoints = 0;
        pressurePoints += totalMemoryGiB < 12 ? 3 : 0;
        pressurePoints += availableMemoryGiB < 3 ? 2 : 0;
        pressurePoints += processors <= 4 ? 2 : 0;
        pressurePoints += freeDiskGiB < 12 ? 2 : 0;
        pressurePoints += gpuWasIdentified ? 0 : 1;
        var pressure = pressurePoints >= 4
            ? PerformancePressureLevel.High
            : pressurePoints >= 1
                ? PerformancePressureLevel.Moderate
                : PerformancePressureLevel.Low;

        var recommendation = pressure == PerformancePressureLevel.High
            ? OptimizationProfile.Aggressive
            : pressure == PerformancePressureLevel.Low
              && totalMemoryGiB >= 24
              && availableMemoryGiB >= 8
              && processors >= 12
              && freeDiskGiB >= 30
                ? OptimizationProfile.Light
                : OptimizationProfile.Balanced;

        return new HardwareProfileAssessment(score, recommendation, pressure);
    }
}
