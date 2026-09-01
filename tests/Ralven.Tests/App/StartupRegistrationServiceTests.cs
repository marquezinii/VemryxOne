using Ralven.App.Services;
using Microsoft.Win32;
using Xunit;

namespace Ralven.Tests.App;

public sealed class StartupRegistrationServiceTests
{
    [Fact]
    public void WindowsStartupRegistration_UsesTheRalvenValueName()
    {
        Assert.Equal("Ralven", WindowsStartupRegistrationService.ValueName);
    }

    [Fact]
    public void WindowsStartupRegistration_QuotesExecutableAndUsesFixedArgument()
    {
        var service = new WindowsStartupRegistrationService(
            @"C:\Program Files\Ralven\Ralven.exe",
            () => throw new InvalidOperationException("Registry access was not expected."));

        Assert.Equal(
            "\"C:\\Program Files\\Ralven\\Ralven.exe\" --startup",
            service.BuildCommand());
    }

    [Theory]
    [InlineData(@"C:\Ralven\Ralven.dll")]
    [InlineData("Ralven.exe")]
    public void WindowsStartupRegistration_RejectsInvalidExecutablePath(string executablePath)
    {
        Assert.Throws<ArgumentException>(() =>
            new WindowsStartupRegistrationService(
                executablePath,
                () => Registry.CurrentUser));
    }

    [Fact]
    public void SessionStartupRegistration_RemainsInMemoryOnly()
    {
        var service = new SessionStartupRegistrationService();

        service.SetEnabled(true);
        Assert.True(service.IsEnabled());

        service.SetEnabled(false);
        Assert.False(service.IsEnabled());
    }
}
