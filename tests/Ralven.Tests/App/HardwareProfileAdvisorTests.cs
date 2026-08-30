using Ralven.App.Services;
using Ralven.Contracts;
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
            logicalProcessorCount: 16,
            freeDiskGiB: 100,
            gpuWasIdentified: true);

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
            logicalProcessorCount: 4,
            freeDiskGiB: 9,
            gpuWasIdentified: true);

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
            logicalProcessorCount: 8,
            freeDiskGiB: 40,
            gpuWasIdentified: true);

        Assert.Equal(OptimizationProfile.Balanced, result.RecommendedProfile);
        Assert.Equal(PerformancePressureLevel.Low, result.PerformancePressure);
    }

    [Fact]
    public void Assess_UsesOnlyGeneralPcCapacitySignals()
    {
        var result = HardwareProfileAdvisor.Assess(
            totalMemoryGiB: 16,
            availableMemoryGiB: 6,
            logicalProcessorCount: 8,
            freeDiskGiB: 40,
            gpuWasIdentified: false);

        Assert.Equal(76, result.ReadinessScore);
        Assert.Equal(OptimizationProfile.Balanced, result.RecommendedProfile);
    }
}
