using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.Windows;

public sealed class WindowsApplicationInventoryInspectorTests
{
    [Fact]
    public async Task InspectAsync_NormalizesDeduplicatesAndSortsInventory()
    {
        var observedAtUtc = new DateTimeOffset(2026, 8, 30, 12, 0, 0, TimeSpan.Zero);
        var result = new WindowsApplicationInventoryReadResult(
            [
                new("  Zebra\tTool  ", " 2.0 ", " Vendor ", 12,
                    WindowsApplicationScope.LocalMachine, WindowsApplicationArchitecture.X64),
                new("alpha app", null, null, null,
                    WindowsApplicationScope.CurrentUser, WindowsApplicationArchitecture.X86),
                new("ZEBRA TOOL", "2.0", "vendor", 12,
                    WindowsApplicationScope.LocalMachine, WindowsApplicationArchitecture.X86),
                new("Zebra Tool", "2.0", "Vendor", 12,
                    WindowsApplicationScope.CurrentUser, WindowsApplicationArchitecture.X64),
                new("   ", null, null, null,
                    WindowsApplicationScope.LocalMachine, WindowsApplicationArchitecture.Unknown)
            ],
            [
                new("  Worker  ", " CurrentUser:RegistryRun ",
                    WindowsStartupItemSource.RegistryRun, WindowsApplicationScope.CurrentUser),
                new("worker", "CurrentUser:RegistryRun",
                    WindowsStartupItemSource.RegistryRun, WindowsApplicationScope.CurrentUser),
                new("Agent", "LocalMachine:StartupFolder",
                    WindowsStartupItemSource.StartupFolder, WindowsApplicationScope.LocalMachine)
            ],
            InstalledApplicationsComplete: true,
            StartupItemsComplete: true);
        var inspector = new WindowsApplicationInventoryInspector(_ => result, () => observedAtUtc);

        var snapshot = await inspector.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Collection(
            snapshot.InstalledApplications,
            application => Assert.Equal("alpha app", application.DisplayName),
            application =>
            {
                Assert.Equal("Zebra Tool", application.DisplayName);
                Assert.Equal("2.0", application.DisplayVersion);
                Assert.Equal("Vendor", application.Publisher);
                Assert.Equal(12 * 1024, application.EstimatedSizeBytes);
                Assert.Equal(WindowsApplicationScope.LocalMachine, application.Scope);
            },
            application => Assert.Equal(
                WindowsApplicationScope.CurrentUser,
                application.Scope));
        Assert.Collection(
            snapshot.StartupItems,
            item => Assert.Equal("Agent", item.Name),
            item => Assert.Equal("Worker", item.Name));
        Assert.Equal(observedAtUtc, snapshot.ObservedAtUtc);
        Assert.False(snapshot.IsPartial);
    }

    [Fact]
    public async Task InspectAsync_PreservesPartialResultInsteadOfReportingMissingData()
    {
        var result = new WindowsApplicationInventoryReadResult(
            [new("Visible app", null, null, null,
                WindowsApplicationScope.LocalMachine, WindowsApplicationArchitecture.X64)],
            [],
            InstalledApplicationsComplete: false,
            StartupItemsComplete: true);
        var inspector = new WindowsApplicationInventoryInspector(_ => result);

        var snapshot = await inspector.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Single(snapshot.InstalledApplications);
        Assert.False(snapshot.InstalledApplicationsComplete);
        Assert.True(snapshot.StartupItemsComplete);
        Assert.True(snapshot.IsPartial);
    }

    [Fact]
    public async Task InspectAsync_HonorsCancellationBeforeReadingInventory()
    {
        var readCalled = false;
        var inspector = new WindowsApplicationInventoryInspector(_ =>
        {
            readCalled = true;
            return new WindowsApplicationInventoryReadResult([], [], true, true);
        });
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => inspector.InspectAsync(cancellation.Token));

        Assert.False(readCalled);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(0L, null)]
    [InlineData(-1L, null)]
    [InlineData(1L, 1024L)]
    [InlineData(17179869184L, 17592186044416L)]
    [InlineData(17179869185L, null)]
    public void ConvertEstimatedSizeToBytes_ValidatesAndCapsRegistryValue(
        long? sizeKib,
        long? expectedBytes)
    {
        Assert.Equal(
            expectedBytes,
            WindowsApplicationInventoryInspector.ConvertEstimatedSizeToBytes(sizeKib));
    }
}
