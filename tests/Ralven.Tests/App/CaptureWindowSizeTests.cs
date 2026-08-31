using Ralven.App;
using Xunit;

namespace Ralven.Tests.App;

public sealed class CaptureWindowSizeTests
{
    [Theory]
    [InlineData("1040x620", 1040, 620)]
    [InlineData(" 1440 X 900 ", 1440, 900)]
    public void TryParseCaptureSize_ValidSize_ReturnsDimensions(string value, int expectedWidth, int expectedHeight)
    {
        Assert.True(MainWindow.TryParseCaptureSize(value, out var width, out var height));
        Assert.Equal(expectedWidth, width);
        Assert.Equal(expectedHeight, height);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1040")]
    [InlineData("0x620")]
    [InlineData("99999x620")]
    public void TryParseCaptureSize_InvalidSize_IsRejected(string value)
    {
        Assert.False(MainWindow.TryParseCaptureSize(value, out _, out _));
    }
}
