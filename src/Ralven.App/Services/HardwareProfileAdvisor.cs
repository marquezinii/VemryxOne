using Ralven.Contracts;
using Ralven.Windows.Infrastructure;

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
        double freeDiskGiB,
        CpuSnapshot? cpu,
        IReadOnlyList<GpuAdapterDetails> gpus)
    {
        ArgumentNullException.ThrowIfNull(gpus);

        var hasDiscreteGpu = gpus.Any(gpu => gpu.KindGuess == GpuKindGuess.LikelyDiscrete);
        var hasOnlyIntegratedGpu = gpus.Count > 0
            && gpus.All(gpu => gpu.KindGuess == GpuKindGuess.LikelyIntegrated);
        var bestDiscreteVramBytes = gpus
            .Where(gpu => gpu.KindGuess == GpuKindGuess.LikelyDiscrete)
            .Select(gpu => gpu.VramBytes ?? 0)
            .DefaultIfEmpty()
            .Max();

        var score = totalMemoryGiB >= 16 ? 25 : totalMemoryGiB >= 8 ? 15 : 7;
        score += availableMemoryGiB >= 8 ? 15 : availableMemoryGiB >= 4 ? 10 : availableMemoryGiB >= 2 ? 6 : 2;
        score += cpu is null
            ? 5
            : cpu.PhysicalCores >= 8 ? 20 : cpu.PhysicalCores >= 6 ? 16 : cpu.PhysicalCores >= 4 ? 10 : 5;
        score += freeDiskGiB >= 30 ? 20 : freeDiskGiB >= 15 ? 13 : freeDiskGiB >= 8 ? 7 : 3;
        score += hasDiscreteGpu
            ? bestDiscreteVramBytes >= 8L * 1024 * 1024 * 1024 ? 20
                : bestDiscreteVramBytes >= 4L * 1024 * 1024 * 1024 ? 15
                : 10
            : hasOnlyIntegratedGpu ? 8 : gpus.Count > 0 ? 10 : 5;
        score = Math.Clamp(score, 0, 100);

        var pressurePoints = 0;
        pressurePoints += totalMemoryGiB < 12 ? 3 : 0;
        pressurePoints += availableMemoryGiB < 3 ? 2 : 0;
        pressurePoints += cpu is not null && cpu.PhysicalCores <= 4 ? 2 : 0;
        pressurePoints += freeDiskGiB < 12 ? 2 : 0;
        pressurePoints += hasOnlyIntegratedGpu ? 2 : 0;
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
              && cpu is { PhysicalCores: >= 8, LogicalThreads: >= 12 }
              && freeDiskGiB >= 30
              && hasDiscreteGpu
              && bestDiscreteVramBytes >= 8L * 1024 * 1024 * 1024
                ? OptimizationProfile.Light
                : OptimizationProfile.Balanced;

        return new HardwareProfileAssessment(score, recommendation, pressure);
    }
}
