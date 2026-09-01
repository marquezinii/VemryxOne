using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Ralven.Contracts;
using Ralven.Core.Planning;
using Xunit;

namespace Ralven.Tests.Core;

public sealed class ContractJsonTests
{
    [Fact]
    public void Request_RoundTripsWithCamelCaseStringEnums()
    {
        var request = new OptimizationPlanRequestDto
        {
            Profile = OptimizationProfile.Balanced,
            Edition = FiveMEdition.Legacy,
            Options = new OptimizationOptionsDto
            {
                ServerCacheRepair = CacheRepairPolicy.WhenOversized,
                ServerCacheThresholdGiB = 12
            }
        };

        var json = RalvenJson.SerializeRequest(request);
        var restored = RalvenJson.DeserializeRequest(json);

        Assert.Contains("\"profile\":\"balanced\"", json, StringComparison.Ordinal);
        Assert.Contains("\"scope\":\"fiveMLegacy\"", json, StringComparison.Ordinal);
        Assert.Contains("\"edition\":\"legacy\"", json, StringComparison.Ordinal);
        Assert.Contains("\"serverCacheRepair\":\"whenOversized\"", json, StringComparison.Ordinal);
        Assert.Equal(request, restored);
    }

    [Fact]
    public void Request_WithoutScopeRetainsLegacyBehavior()
    {
        const string json = """
            {
              "profile": "light",
              "edition": "legacy",
              "options": {}
            }
            """;

        var restored = RalvenJson.DeserializeRequest(json);

        Assert.Equal(OptimizationScope.FiveMLegacy, restored.Scope);
    }

    [Fact]
    public void Plan_RoundTripsWithActionMetadata()
    {
        var original = PlanBuilder.Build(
            new OptimizationPlanRequestDto
            {
                Profile = OptimizationProfile.Aggressive,
                Edition = FiveMEdition.Legacy
            },
            PlanBuildContext.New(TimeProvider.System));

        var json = RalvenJson.SerializePlan(original);
        var restored = RalvenJson.DeserializePlan(json);

        Assert.Equal(original.PlanId, restored.PlanId);
        Assert.Equal(original.SchemaVersion, restored.SchemaVersion);
        Assert.Equal(original.CatalogVersion, restored.CatalogVersion);
        Assert.Equal(original.ProductName, restored.ProductName);
        Assert.Equal(original.ProductSubtitle, restored.ProductSubtitle);
        Assert.Equal(original.Scope, restored.Scope);
        Assert.Equal(original.Profile, restored.Profile);
        Assert.Equal(original.Edition, restored.Edition);
        Assert.Equal(
            original.Actions.Select(action => action.Metadata.Id),
            restored.Actions.Select(action => action.Metadata.Id));
        Assert.NotEmpty(original.Notices);
        Assert.Equal(original.Notices, restored.Notices);
        Assert.Equal(original.Options, restored.Options);
    }

    [Fact]
    public void Plan_WithoutScopeRetainsLegacyBehavior()
    {
        var original = PlanBuilder.Build(
            new OptimizationPlanRequestDto
            {
                Profile = OptimizationProfile.Light,
                Edition = FiveMEdition.Legacy
            },
            PlanBuildContext.New(TimeProvider.System));
        var root = JsonNode.Parse(RalvenJson.SerializePlan(original))!.AsObject();
        root.Remove("scope");

        var restored = RalvenJson.DeserializePlan(root.ToJsonString());

        Assert.Equal(OptimizationScope.FiveMLegacy, restored.Scope);
    }

    [Fact]
    public void Plan_RoundTripsBlocksOfANonExecutablePlan()
    {
        var original = PlanBuilder.Build(
            new OptimizationPlanRequestDto
            {
                Profile = OptimizationProfile.Light,
                Edition = FiveMEdition.Enhanced
            },
            PlanBuildContext.New(TimeProvider.System));

        var restored = RalvenJson.DeserializePlan(RalvenJson.SerializePlan(original));

        Assert.False(restored.IsExecutable);
        Assert.NotEmpty(original.Blocks);
        Assert.Equal(original.Blocks, restored.Blocks);
    }

    [Fact]
    public void SharedOptions_CannotBeMutatedByConsumers()
    {
        Assert.True(RalvenJson.Options.IsReadOnly);
        Assert.Throws<InvalidOperationException>(() =>
            RalvenJson.Options.Converters.Add(new JsonStringEnumConverter()));
    }

    [Fact]
    public void UnknownJsonMembers_AreRejected()
    {
        const string json = """
            {
              "profile": "light",
              "edition": "legacy",
              "options": {},
              "command": "powershell -encodedCommand unsafe"
            }
            """;

        Assert.Throws<JsonException>(() => RalvenJson.DeserializeRequest(json));
    }

    [Fact]
    public void NumericEnums_AreRejected()
    {
        const string json = """
            {
              "profile": 1,
              "edition": "legacy",
              "options": {}
            }
            """;

        Assert.Throws<JsonException>(() => RalvenJson.DeserializeRequest(json));
    }

    [Fact]
    public void EmptyPayload_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => RalvenJson.DeserializeRequest(" "));
    }
}
