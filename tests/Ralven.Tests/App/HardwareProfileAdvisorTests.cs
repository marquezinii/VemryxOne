using Ralven.App.Services;
using Ralven.Contracts;
using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.App;

public sealed class HardwareProfileAdvisorTests
{
    [Fact]
    public void Assess_RecommendsLightOnlyForLowPressureHighCapacityPc()
    {
        var result = HardwareProfileAdvisor.Assess(
            totalMemoryGiB: 32,
            availableMemoryGiB: 12,
            freeDiskGiB: 100,
            cpu: new CpuSnapshot(8, 16, 4200, 5000),
            gpus: [new GpuAdapterDetails("Discrete GPU", 12L * 1024 * 1024 * 1024, GpuKindGuess.LikelyDiscrete)]);

        Assert.Equal(OptimizationProfile.Light, result.RecommendedProfile);
        Assert.Equal(PerformancePressureLevel.Low, result.PerformancePressure);
        Assert.Equal(100, result.ReadinessScore);
    }

    [Fact]
    public void Assess_RecommendsAggressiveForSeveralMeasuredConstraints()
    {
        var result = HardwareProfileAdvisor.Assess(
            totalMemoryGiB: 8,
            availableMemoryGiB: 1.5,
            freeDiskGiB: 9,
            cpu: new CpuSnapshot(2, 4, 2800, 3400),
            gpus: [new GpuAdapterDetails("Integrated GPU", 1L * 1024 * 1024 * 1024, GpuKindGuess.LikelyIntegrated)]);

        Assert.Equal(OptimizationProfile.Aggressive, result.RecommendedProfile);
        Assert.Equal(PerformancePressureLevel.High, result.PerformancePressure);
        Assert.InRange(result.ReadinessScore, 0, 99);
    }

    [Fact]
    public void Assess_UsesBalancedForTypicalPcWithoutInventingAHighEndResult()
    {
        var result = HardwareProfileAdvisor.Assess(
            totalMemoryGiB: 16,
            availableMemoryGiB: 6,
            freeDiskGiB: 40,
            cpu: new CpuSnapshot(6, 12, 3600, 4400),
            gpus: [new GpuAdapterDetails("Discrete GPU", 6L * 1024 * 1024 * 1024, GpuKindGuess.LikelyDiscrete)]);

        Assert.Equal(OptimizationProfile.Balanced, result.RecommendedProfile);
        Assert.Equal(PerformancePressureLevel.Low, result.PerformancePressure);
    }

    [Fact]
    public void Assess_UsesBalancedWhenHardwareDetailsAreUnavailable()
    {
        var result = HardwareProfileAdvisor.Assess(
            totalMemoryGiB: 16,
            availableMemoryGiB: 6,
            freeDiskGiB: 40,
            cpu: null,
            gpus: []);

        Assert.Equal(OptimizationProfile.Balanced, result.RecommendedProfile);
        Assert.Equal(PerformancePressureLevel.Low, result.PerformancePressure);
        Assert.InRange(result.ReadinessScore, 0, 99);
    }
}
