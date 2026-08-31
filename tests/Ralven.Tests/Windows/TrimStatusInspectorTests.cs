using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.Windows;

public sealed class TrimStatusInspectorTests
{
    [Fact]
    public async Task InspectAsync_ParsesNumericNtfsAndRefsStatesWithoutLocalizedLabels()
    {
        var runner = new StubRunner(new CommandResult(
            0,
            "NTFS texto localizado = 0 (qualquer texto)\r\nReFS autre texte = 1 (texte)",
            string.Empty));
        var inspector = new WindowsTrimStatusInspector(runner);

        var snapshot = await inspector.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal(TrimInspectionState.Available, snapshot.State);
        Assert.Equal(TrimDeleteNotificationState.Enabled, snapshot.Ntfs);
        Assert.Equal(TrimDeleteNotificationState.Disabled, snapshot.ReFs);
        Assert.EndsWith("fsutil.exe", runner.Executable, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(["behavior", "query", "DisableDeleteNotify"], runner.Arguments);
    }

    [Fact]
    public async Task InspectAsync_ReportsPartialWhenOnlyOneFilesystemHasANumericState()
    {
        var inspector = new WindowsTrimStatusInspector(new StubRunner(new CommandResult(
            0,
            "NTFS DisableDeleteNotify = 1\r\nReFS DisableDeleteNotify is not currently set",
            string.Empty)));

        var snapshot = await inspector.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal(TrimInspectionState.Partial, snapshot.State);
        Assert.Equal(TrimDeleteNotificationState.Disabled, snapshot.Ntfs);
        Assert.Null(snapshot.ReFs);
    }

    [Fact]
    public async Task InspectAsync_RejectsUndocumentedNumericStates()
    {
        var inspector = new WindowsTrimStatusInspector(new StubRunner(new CommandResult(
            0,
            "NTFS DisableDeleteNotify = 2",
            string.Empty)));

        var snapshot = await inspector.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal(TrimInspectionState.Unavailable, snapshot.State);
        Assert.Null(snapshot.Ntfs);
    }

    [Theory]
    [InlineData(5, TrimInspectionState.AccessDenied)]
    [InlineData(50, TrimInspectionState.Unsupported)]
    [InlineData(1, TrimInspectionState.Unavailable)]
    public async Task InspectAsync_ClassifiesFailedQueriesWithoutThrowing(
        int exitCode,
        TrimInspectionState expected)
    {
        var inspector = new WindowsTrimStatusInspector(new StubRunner(
            new CommandResult(exitCode, string.Empty, "localized failure")));

        var snapshot = await inspector.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal(expected, snapshot.State);
        Assert.Null(snapshot.Ntfs);
        Assert.Null(snapshot.ReFs);
    }

    [Theory]
    [InlineData("NTFS DisableDeleteNotify = 0x")]
    [InlineData("NTFS DisableDeleteNotify = 1unknown")]
    public async Task InspectAsync_RejectsNonWhitespaceSuffixAfterNumericState(string output)
    {
        var inspector = new WindowsTrimStatusInspector(new StubRunner(new CommandResult(
            0,
            output,
            string.Empty)));

        var snapshot = await inspector.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal(TrimInspectionState.Unavailable, snapshot.State);
        Assert.Null(snapshot.Ntfs);
        Assert.Null(snapshot.ReFs);
    }

    [Fact]
    public async Task InspectAsync_ReportsUnavailableWhenTheRunnerThrows()
    {
        var inspector = new WindowsTrimStatusInspector(new StubRunner(
            new InvalidOperationException("runner failure")));

        var snapshot = await inspector.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal(TrimInspectionState.Unavailable, snapshot.State);
    }

    [Theory]
    [MemberData(nameof(ClassifiedExceptions))]
    public async Task InspectAsync_ClassifiesExpectedRunnerExceptions(
        Exception exception,
        TrimInspectionState expected)
    {
        var inspector = new WindowsTrimStatusInspector(new StubRunner(exception));

        var snapshot = await inspector.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal(expected, snapshot.State);
    }

    public static TheoryData<Exception, TrimInspectionState> ClassifiedExceptions => new()
    {
        { new UnauthorizedAccessException(), TrimInspectionState.AccessDenied },
        { new PlatformNotSupportedException(), TrimInspectionState.Unsupported }
    };

    private sealed class StubRunner : ICommandRunner
    {
        private readonly CommandResult? result;
        private readonly Exception? exception;

        public StubRunner(CommandResult result)
        {
            this.result = result;
        }

        public StubRunner(Exception exception)
        {
            this.exception = exception;
        }

        public string? Executable { get; private set; }
        public IReadOnlyList<string>? Arguments { get; private set; }

        public Task<CommandResult> RunAsync(
            string executable,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            Executable = executable;
            Arguments = arguments;
            return exception is null
                ? Task.FromResult(result!)
                : Task.FromException<CommandResult>(exception);
        }
    }
}
