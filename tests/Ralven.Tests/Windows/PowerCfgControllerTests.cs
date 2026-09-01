using Ralven.Windows.Actions;
using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.Windows;

public sealed class PowerCfgControllerTests
{
    [Fact]
    public async Task Controller_AlwaysUsesAbsoluteSystem32Executable()
    {
        var scheme = Guid.NewGuid();
        var runner = new StubRunner(_ => new CommandResult(
            0,
            $"Power Scheme GUID: {scheme:D} (Balanced)",
            string.Empty));
        var controller = new PowerCfgController(runner);

        var actual = await controller.GetActiveSchemeAsync(CancellationToken.None);

        Assert.Equal(scheme, actual);
        Assert.Equal(
            Path.GetFullPath(Path.Combine(Environment.SystemDirectory, "powercfg.exe")),
            runner.Executable);
        Assert.True(Path.IsPathFullyQualified(runner.Executable));
    }

    [Fact]
    public async Task GetPciExpressAspmPolicyAsync_PropagatesActiveSchemeLookupFailure()
    {
        var runner = new StubRunner(_ => new CommandResult(1, string.Empty, "error"));
        var controller = new PowerCfgController(runner);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => controller.GetPciExpressAspmPolicyAsync(CancellationToken.None));
    }

    [Fact]
    public async Task GetPciExpressAspmPolicyAsync_ReturnsAcAndDcValues()
    {
        var scheme = Guid.NewGuid();
        var runner = new StubRunner(_ => new CommandResult(
            0,
            $"Power Scheme GUID: {scheme:D} (Balanced)",
            string.Empty));
        var controller = new PowerCfgController(runner, _ => new PciExpressAspmPolicy(1, 2));

        var value = await controller.GetPciExpressAspmPolicyAsync(CancellationToken.None);

        Assert.Equal(new PciExpressAspmState(scheme, new PciExpressAspmPolicy(1, 2)), value);
    }

    [Theory]
    [InlineData(2, PowerPlanActivationOutcome.SchemeUnavailable)]
    [InlineData(1, PowerPlanActivationOutcome.Failed)]
    public async Task TryActivatePerformanceSchemeAsync_OnlyMissingSchemeIsUnavailable(
        int exitCode,
        PowerPlanActivationOutcome expected)
    {
        var controller = new PowerCfgController(
            new StubRunner(_ => new CommandResult(exitCode, string.Empty, "simulated failure")));

        var outcome = await controller.TryActivatePerformanceSchemeAsync(CancellationToken.None);

        Assert.Equal(expected, outcome);
    }

    [Fact]
    public async Task SetPciExpressAspmPolicyAsync_SetsBothAcAndDcThenAppliesTheScheme()
    {
        var calls = new List<IReadOnlyList<string>>();
        var scheme = Guid.NewGuid();
        var policy = new PciExpressAspmPolicy(1, 2);
        var runner = new StubRunner(arguments =>
        {
            calls.Add(arguments);
            if (arguments.Contains("/GETACTIVESCHEME"))
            {
                return new CommandResult(0, $"Power Scheme GUID: {scheme:D} (Balanced)", string.Empty);
            }

            if (arguments.Contains("/setacvalueindex"))
            {
                policy = policy with { AcPolicy = int.Parse(arguments[^1], System.Globalization.CultureInfo.InvariantCulture) };
            }
            else if (arguments.Contains("/setdcvalueindex"))
            {
                policy = policy with { DcPolicy = int.Parse(arguments[^1], System.Globalization.CultureInfo.InvariantCulture) };
            }

            return new CommandResult(0, string.Empty, string.Empty);
        });
        var controller = new PowerCfgController(runner, _ => policy);

        await controller.SetPciExpressAspmPolicyAsync(
            scheme,
            new PciExpressAspmPolicy(1, 2),
            new PciExpressAspmPolicy(0, 0),
            CancellationToken.None);

        Assert.Equal(new PciExpressAspmPolicy(0, 0), policy);
        Assert.Contains(calls, arguments => arguments.Contains("/setacvalueindex"));
        Assert.Contains(calls, arguments => arguments.Contains("/setdcvalueindex"));
        Assert.Contains(calls, arguments => arguments.Contains("/S"));
        Assert.DoesNotContain(calls, arguments => arguments.Contains("SCHEME_CURRENT"));
        Assert.All(
            calls.Where(arguments => arguments.Any(argument => argument.StartsWith("/set", StringComparison.OrdinalIgnoreCase))
                || arguments.Contains("/S")),
            arguments => Assert.Contains(scheme.ToString("D"), arguments));
    }

    [Theory]
    [InlineData("/setdcvalueindex")]
    [InlineData("/S")]
    public async Task SetPciExpressAspmPolicyAsync_CompensatesPartialFailure(string failingArgument)
    {
        var scheme = Guid.NewGuid();
        var original = new PciExpressAspmPolicy(1, 2);
        var policy = original;
        var failed = false;
        var runner = new StubRunner(arguments =>
        {
            if (arguments.Contains("/GETACTIVESCHEME"))
            {
                return new CommandResult(0, $"Power Scheme GUID: {scheme:D} (Balanced)", string.Empty);
            }

            if (!failed && arguments.Contains(failingArgument))
            {
                failed = true;
                return new CommandResult(1, string.Empty, "simulated failure");
            }

            if (arguments.Contains("/setacvalueindex"))
            {
                policy = policy with { AcPolicy = int.Parse(arguments[^1], System.Globalization.CultureInfo.InvariantCulture) };
            }
            else if (arguments.Contains("/setdcvalueindex"))
            {
                policy = policy with { DcPolicy = int.Parse(arguments[^1], System.Globalization.CultureInfo.InvariantCulture) };
            }

            return new CommandResult(0, string.Empty, string.Empty);
        });
        var controller = new PowerCfgController(runner, _ => policy);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            controller.SetPciExpressAspmPolicyAsync(
                scheme,
                original,
                new PciExpressAspmPolicy(0, 0),
                CancellationToken.None));

        Assert.Equal(original, policy);
    }

    [Fact]
    public async Task SetPciExpressAspmPolicyAsync_RejectsOutOfRangeValues()
    {
        var runner = new StubRunner(_ => new CommandResult(0, string.Empty, string.Empty));
        var controller = new PowerCfgController(runner, _ => new PciExpressAspmPolicy(1, 1));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => controller.SetPciExpressAspmPolicyAsync(
                Guid.NewGuid(),
                new PciExpressAspmPolicy(1, 1),
                new PciExpressAspmPolicy(3, 0),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetPciExpressAspmPolicyAsync_PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var controller = new PowerCfgController(new CancellingRunner(), _ => new PciExpressAspmPolicy(1, 1));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            controller.GetPciExpressAspmPolicyAsync(cancellation.Token));
    }

    [Fact]
    public async Task SetPciExpressAspmPolicyAsync_CancellationCompensatesAndPropagates()
    {
        using var cancellation = new CancellationTokenSource();
        var original = new PciExpressAspmPolicy(1, 2);
        var runner = new CancelOnFirstDcRunner(original, cancellation);
        var controller = new PowerCfgController(runner, _ => runner.Policy);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            controller.SetPciExpressAspmPolicyAsync(
                runner.Scheme,
                original,
                new PciExpressAspmPolicy(0, 0),
                cancellation.Token));

        Assert.Equal(original, runner.Policy);
    }

    [Fact]
    public async Task SetPciExpressAspmPolicyAsync_SchemeChangeFailsClosedAndCompensatesExactScheme()
    {
        var originalScheme = Guid.NewGuid();
        var newerScheme = Guid.NewGuid();
        var activeScheme = originalScheme;
        var policy = new PciExpressAspmPolicy(1, 2);
        var calls = new List<IReadOnlyList<string>>();
        var runner = new StubRunner(arguments =>
        {
            calls.Add(arguments);
            if (arguments.Contains("/GETACTIVESCHEME"))
            {
                return new CommandResult(0, $"Power Scheme GUID: {activeScheme:D}", string.Empty);
            }

            if (arguments.Contains("/setacvalueindex"))
            {
                policy = policy with { AcPolicy = int.Parse(arguments[^1], System.Globalization.CultureInfo.InvariantCulture) };
                if (activeScheme == originalScheme)
                {
                    activeScheme = newerScheme;
                }
            }

            return new CommandResult(0, string.Empty, string.Empty);
        });
        var controller = new PowerCfgController(runner, _ => policy);

        await Assert.ThrowsAsync<IOException>(() => controller.SetPciExpressAspmPolicyAsync(
            originalScheme,
            new PciExpressAspmPolicy(1, 2),
            new PciExpressAspmPolicy(0, 0),
            CancellationToken.None));

        Assert.Equal(new PciExpressAspmPolicy(1, 2), policy);
        Assert.Equal(newerScheme, activeScheme);
        Assert.DoesNotContain(calls, arguments => arguments.Contains("/S"));
        Assert.All(
            calls.Where(arguments => arguments.Contains("/setacvalueindex")),
            arguments => Assert.Contains(originalScheme.ToString("D"), arguments));
    }

    [Fact]
    public async Task SetPciExpressAspmPolicyAsync_CompensationPreservesConcurrentPolicyValue()
    {
        var scheme = Guid.NewGuid();
        var policy = new PciExpressAspmPolicy(1, 2);
        var runner = new StubRunner(arguments =>
        {
            if (arguments.Contains("/GETACTIVESCHEME"))
            {
                return new CommandResult(0, $"Power Scheme GUID: {scheme:D}", string.Empty);
            }

            if (arguments.Contains("/setacvalueindex"))
            {
                policy = policy with { AcPolicy = int.Parse(arguments[^1], System.Globalization.CultureInfo.InvariantCulture) };
            }
            else if (arguments.Contains("/setdcvalueindex"))
            {
                policy = policy with { AcPolicy = 2 };
                return new CommandResult(1, string.Empty, "simulated failure");
            }

            return new CommandResult(0, string.Empty, string.Empty);
        });
        var controller = new PowerCfgController(runner, _ => policy);

        await Assert.ThrowsAsync<AggregateException>(() => controller.SetPciExpressAspmPolicyAsync(
            scheme,
            new PciExpressAspmPolicy(1, 2),
            new PciExpressAspmPolicy(0, 0),
            CancellationToken.None));

        Assert.Equal(new PciExpressAspmPolicy(2, 2), policy);
    }

    [Theory]
    [InlineData("PowerReadACValueIndex")]
    [InlineData("PowerReadDCValueIndex")]
    public void NativePolicyReader_PassesGuidsByReadonlyReference(string methodName)
    {
        var method = typeof(PowerCfgController).GetMethod(
            methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(method);
        Assert.All(
            method!.GetParameters().Skip(1).Take(3),
            parameter => Assert.Equal(typeof(Guid).MakeByRefType(), parameter.ParameterType));
    }

    private sealed class StubRunner(
        Func<IReadOnlyList<string>, CommandResult> respond) : ICommandRunner
    {
        public string Executable { get; private set; } = string.Empty;

        public Task<CommandResult> RunAsync(
            string executable,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            Executable = executable;
            return Task.FromResult(respond(arguments));
        }
    }

    private sealed class CancellingRunner : ICommandRunner
    {
        public Task<CommandResult> RunAsync(
            string executable,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken) => Task.FromCanceled<CommandResult>(cancellationToken);
    }

    private sealed class CancelOnFirstDcRunner(
        PciExpressAspmPolicy initialPolicy,
        CancellationTokenSource cancellation) : ICommandRunner
    {
        private bool cancellationTriggered;

        public PciExpressAspmPolicy Policy { get; private set; } = initialPolicy;

        public Guid Scheme { get; } = Guid.NewGuid();

        public Task<CommandResult> RunAsync(
            string executable,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (arguments.Contains("/GETACTIVESCHEME"))
            {
                return Task.FromResult(new CommandResult(
                    0,
                    $"Power Scheme GUID: {Scheme:D} (Balanced)",
                    string.Empty));
            }

            if (arguments.Contains("/setacvalueindex"))
            {
                Policy = Policy with
                {
                    AcPolicy = int.Parse(arguments[^1], System.Globalization.CultureInfo.InvariantCulture)
                };
            }
            else if (arguments.Contains("/setdcvalueindex") && !cancellationTriggered)
            {
                cancellationTriggered = true;
                cancellation.Cancel();
                return Task.FromCanceled<CommandResult>(cancellation.Token);
            }
            else if (arguments.Contains("/setdcvalueindex"))
            {
                Policy = Policy with
                {
                    DcPolicy = int.Parse(arguments[^1], System.Globalization.CultureInfo.InvariantCulture)
                };
            }

            return Task.FromResult(new CommandResult(0, string.Empty, string.Empty));
        }
    }
}
