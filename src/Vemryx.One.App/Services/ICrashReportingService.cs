namespace Vemryx.One.App.Services;

/// <summary>
/// Abstraction over the crash reporting SDK (Sentry), kept separate from
/// <c>SentrySdk</c>'s static API so callers and tests never depend on the
/// concrete SDK. Only the <c>App</c> layer references this — never
/// <c>Core</c>, <c>Windows</c>, or <c>Broker</c> — and it is never
/// initialized before the user's privacy consent authorizes it.
/// </summary>
public interface ICrashReportingService
{
    bool IsInitialized { get; }

    void Initialize(RemoteServicesOptions options, string appVersion);

    void CaptureException(Exception exception);

    void Shutdown();
}

/// <summary>
/// Default, inert implementation: every call is a no-op. This is what
/// <see cref="CrashReporting.Current"/> holds until the app explicitly
/// initializes a real service, so any code path that runs before consent is
/// resolved (or in demo mode) safely does nothing instead of needing null
/// checks everywhere.
/// </summary>
public sealed class NoOpCrashReportingService : ICrashReportingService
{
    public static NoOpCrashReportingService Instance { get; } = new();

    private NoOpCrashReportingService()
    {
    }

    public bool IsInitialized => false;

    public void Initialize(RemoteServicesOptions options, string appVersion)
    {
    }

    public void CaptureException(Exception exception)
    {
    }

    public void Shutdown()
    {
    }
}

/// <summary>
/// Process-wide holder for the active crash reporting service, mirroring the
/// existing <c>LocalizationService.Current</c> singleton pattern used
/// elsewhere in this layer. Starts as <see cref="NoOpCrashReportingService"/>
/// so <c>App.xaml.cs</c>'s static unhandled-exception handlers — which run
/// before any window or view model exists — always have a safe target to
/// call, whether or not crash reporting ends up authorized this run.
/// </summary>
public static class CrashReporting
{
    private static ICrashReportingService current = NoOpCrashReportingService.Instance;

    public static ICrashReportingService Current
    {
        get => current;
        set => current = value ?? throw new ArgumentNullException(nameof(value));
    }
}

/// <summary>
/// Keeps crash reporting fail-closed: initialization only happens after the
/// caller has evaluated current consent, and SDK failures fall back to the
/// inert service without affecting application startup or shutdown.
/// </summary>
internal static class CrashReportingLifecycle
{
    internal static ICrashReportingService InitializeIfAuthorized(
        bool isAuthorized,
        RemoteServicesOptions options,
        string appVersion,
        Func<ICrashReportingService> serviceFactory)
    {
        if (!isAuthorized)
        {
            return NoOpCrashReportingService.Instance;
        }

        ICrashReportingService? service = null;
        try
        {
            service = serviceFactory();
            service.Initialize(options, appVersion);
            return service;
        }
        catch
        {
            TryShutdown(service);
            return NoOpCrashReportingService.Instance;
        }
    }

    internal static void TryShutdown(ICrashReportingService? service)
    {
        try
        {
            service?.Shutdown();
        }
        catch
        {
            // Crash reporting must never prevent a clean application exit.
        }
    }
}
