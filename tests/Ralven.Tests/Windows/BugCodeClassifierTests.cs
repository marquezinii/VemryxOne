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
}
