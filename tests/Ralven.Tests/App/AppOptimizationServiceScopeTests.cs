using Ralven.App.Services;
using Ralven.Contracts;
using Ralven.Core.Planning;
using Xunit;

namespace Ralven.Tests.App;

public sealed class AppOptimizationServiceScopeTests
{
    [Fact]
    public void CreateRuntimeForPlan_AllowsGeneralWindowsWithoutDetectedFiveM()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var service = new AppOptimizationService(temporaryDirectory.Path);
        var plan = BuildPlan(OptimizationScope.GeneralWindows, FiveMEdition.Unknown);

        var runtime = service.CreateRuntimeForPlan(plan);

        Assert.NotNull(runtime);
    }

    [Fact]
    public void CreateRuntimeForPlan_RequiresDetectedFiveMForFiveMLegacyScope()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var service = new AppOptimizationService(temporaryDirectory.Path);
        var plan = BuildPlan(OptimizationScope.FiveMLegacy, FiveMEdition.Legacy);

        Assert.Throws<InvalidOperationException>(() => service.CreateRuntimeForPlan(plan));
    }

    private static OptimizationPlanDto BuildPlan(OptimizationScope scope, FiveMEdition edition) =>
        PlanBuilder.Build(
            new OptimizationPlanRequestDto
            {
                Scope = scope,
                Profile = OptimizationProfile.Balanced,
                Edition = edition
            },
            PlanBuildContext.New(TimeProvider.System));
}
