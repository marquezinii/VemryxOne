using Ralven.Contracts;
using Ralven.Windows.Diagnostics;
using Xunit;

namespace Ralven.Tests.Windows;

public sealed class BugCodeClassifierTests
{
    [Fact]
    public void ClassifyException_UnrecognizedExceptionType_FallsBackToOptimizationOnlyWhenContextIsOptimization()
    {
        var result = BugCodeClassifier.ClassifyException(new FormatException("boom"), "optimization");

        Assert.Equal(BugCode.APP_OPT_ACTION_EXECUTION, result);
    }

    [Fact]
    public void ClassifyException_UnrecognizedContext_ReturnsUnknownRatherThanOptimization()
    {
        var result = BugCodeClassifier.ClassifyException(new FormatException("boom"), "some-unmapped-context");

        Assert.Equal(BugCode.Unknown, result);
    }

    [Fact]
    public void ClassifyException_AppInventoryContext_ReturnsAppInventoryScan()
    {
        var result = BugCodeClassifier.ClassifyException(new UnauthorizedAccessException(), "app-inventory");

        Assert.Equal(BugCode.APP_INV_SCAN, result);
    }

    [Fact]
    public void ClassifyException_SecurityHealthContext_ReturnsSecurityHealthQuery()
    {
        var result = BugCodeClassifier.ClassifyException(new DllNotFoundException(), "security-health");

        Assert.Equal(BugCode.SEC_HEALTH_QUERY, result);
    }
}
