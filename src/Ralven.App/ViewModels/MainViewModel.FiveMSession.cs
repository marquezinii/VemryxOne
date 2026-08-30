using System.Globalization;
using Ralven.App.Services;
using Ralven.Contracts;
using Ralven.Windows.Infrastructure;

namespace Ralven.App.ViewModels;

public sealed partial class MainViewModel
{
    private static readonly TimeSpan FiveMSessionPollInterval = TimeSpan.FromSeconds(5);
    private readonly Func<string, FiveMSessionPresence> fiveMSessionProbe;
    private readonly FiveMSessionStateTracker fiveMSessionTracker = new();
    private readonly SemaphoreSlim fiveMSessionProbeGate = new(1, 1);
    private CancellationTokenSource? fiveMSessionCancellation;
    private string? fiveMSessionRoot;
    private FiveMSessionPresence? lastFiveMSessionPresence;
    private bool isFiveMSessionMonitoring;
    private string fiveMSessionStatusLabel = string.Empty;
    private string fiveMSessionDetailLabel = string.Empty;
    private string fiveMSessionActionLabel = string.Empty;
    private string fiveMSessionStatusBrushKey = "TextTertiaryBrush";

    public bool IsFiveMSessionMonitoring
    {
        get => isFiveMSessionMonitoring;
        private set => SetProperty(ref isFiveMSessionMonitoring, value);
    }

    public bool IsFiveMSessionActive => fiveMSessionTracker.IsActive;

    public bool IsFiveMSessionEndConfirmationPending => fiveMSessionTracker.IsEndConfirmationPending;

    public bool IsFiveMSessionReadUnavailable => IsFiveMSessionMonitoring
        && lastFiveMSessionPresence == FiveMSessionPresence.Indeterminate;

    public bool CanToggleFiveMSessionMonitor => IsFiveMSessionMonitoring || HasLegacySessionRoot();

    public string FiveMSessionStatusLabel
    {
        get => fiveMSessionStatusLabel;
        private set => SetProperty(ref fiveMSessionStatusLabel, value);
    }

    public string FiveMSessionDetailLabel
    {
        get => fiveMSessionDetailLabel;
        private set => SetProperty(ref fiveMSessionDetailLabel, value);
    }

    public string FiveMSessionActionLabel
    {
        get => fiveMSessionActionLabel;
        private set => SetProperty(ref fiveMSessionActionLabel, value);
    }

    public string FiveMSessionStatusBrushKey
    {
        get => fiveMSessionStatusBrushKey;
        private set => SetProperty(ref fiveMSessionStatusBrushKey, value);
    }

    public void ToggleFiveMSessionMonitor()
    {
        if (IsFiveMSessionMonitoring)
        {
            StopFiveMSessionMonitor();
            return;
        }

        if (!HasLegacySessionRoot())
        {
            RefreshFiveMSessionMonitorPresentation();
            return;
        }

        fiveMSessionRoot = diagnostic!.FiveMRoot;
        fiveMSessionTracker.Reset();
        lastFiveMSessionPresence = null;
        IsFiveMSessionMonitoring = true;
        RefreshFiveMSessionMonitorPresentation();

        var cancellation = new CancellationTokenSource();
        fiveMSessionCancellation = cancellation;
        var monitorTask = MonitorFiveMSessionAsync(cancellation);
        _ = monitorTask.ContinueWith(
            static task => _ = task.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private async Task MonitorFiveMSessionAsync(CancellationTokenSource owner)
    {
        var cancellationToken = owner.Token;
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var presence = await ProbeFiveMSessionAsync(fiveMSessionRoot!, cancellationToken);
                if (cancellationToken.IsCancellationRequested
                    || !ReferenceEquals(fiveMSessionCancellation, owner))
                {
                    return;
                }

                lastFiveMSessionPresence = presence;
                fiveMSessionTracker.Observe(presence, DateTimeOffset.UtcNow);
                RefreshFiveMSessionMonitorPresentation();
                await Task.Delay(FiveMSessionPollInterval, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task<FiveMSessionPresence> ProbeFiveMSessionAsync(
        string legacyRoot,
        CancellationToken cancellationToken)
    {
        var entered = false;
        try
        {
            await fiveMSessionProbeGate.WaitAsync(cancellationToken);
            entered = true;
            return await Task.Run(() => fiveMSessionProbe(legacyRoot), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is not (
            OutOfMemoryException or StackOverflowException or AccessViolationException))
        {
            return FiveMSessionPresence.Indeterminate;
        }
        finally
        {
            if (entered)
            {
                fiveMSessionProbeGate.Release();
            }
        }
    }

    private void StopFiveMSessionMonitor()
    {
        var cancellation = fiveMSessionCancellation;
        fiveMSessionCancellation = null;
        cancellation?.Cancel();
        cancellation?.Dispose();
        fiveMSessionRoot = null;
        lastFiveMSessionPresence = null;
        fiveMSessionTracker.Reset();
        IsFiveMSessionMonitoring = false;
        RefreshFiveMSessionMonitorPresentation();
    }

    private void RefreshFiveMSessionMonitorAvailability()
    {
        if (IsFiveMSessionMonitoring
            && (!HasLegacySessionRoot()
                || !string.Equals(
                    fiveMSessionRoot,
                    diagnostic!.FiveMRoot,
                    StringComparison.OrdinalIgnoreCase)))
        {
            StopFiveMSessionMonitor();
            return;
        }

        RefreshFiveMSessionMonitorPresentation();
    }

    private void RefreshFiveMSessionMonitorPresentation()
    {
        FiveMSessionActionLabel = localization.GetString(
            IsFiveMSessionMonitoring
                ? "Dashboard.SessionMonitor.Stop"
                : "Dashboard.SessionMonitor.Start");

        if (!HasLegacySessionRoot())
        {
            SetFiveMSessionPresentation(
                "Dashboard.SessionMonitor.Unavailable",
                "Dashboard.SessionMonitor.UnavailableDetail",
                "TextTertiaryBrush");
        }
        else if (!IsFiveMSessionMonitoring)
        {
            SetFiveMSessionPresentation(
                "Dashboard.SessionMonitor.Off",
                "Dashboard.SessionMonitor.OffDetail",
                "TextSecondaryBrush");
        }
        else if (IsFiveMSessionReadUnavailable)
        {
            SetFiveMSessionPresentation(
                "Dashboard.SessionMonitor.ReadUnavailable",
                "Dashboard.SessionMonitor.ReadUnavailableDetail",
                "WarningBaseBrush");
        }
        else if (fiveMSessionTracker.IsEndConfirmationPending)
        {
            SetFiveMSessionPresentation(
                "Dashboard.SessionMonitor.ConfirmingEnd",
                "Dashboard.SessionMonitor.ConfirmingEndDetail",
                "WarningBaseBrush");
        }
        else if (fiveMSessionTracker.IsActive)
        {
            FiveMSessionStatusLabel = localization.GetString("Dashboard.SessionMonitor.Active");
            FiveMSessionDetailLabel = localization.Format(
                "Dashboard.SessionMonitor.ActiveDetail",
                FormatSessionDuration(DateTimeOffset.UtcNow - fiveMSessionTracker.StartedAt!.Value));
            FiveMSessionStatusBrushKey = "SuccessBaseBrush";
        }
        else if (fiveMSessionTracker.HasCompletedSession)
        {
            FiveMSessionStatusLabel = localization.GetString("Dashboard.SessionMonitor.Completed");
            FiveMSessionDetailLabel = localization.Format(
                "Dashboard.SessionMonitor.CompletedDetail",
                FormatSessionDuration(fiveMSessionTracker.LastDuration ?? TimeSpan.Zero));
            FiveMSessionStatusBrushKey = "InfoBaseBrush";
        }
        else
        {
            SetFiveMSessionPresentation(
                "Dashboard.SessionMonitor.Waiting",
                "Dashboard.SessionMonitor.WaitingDetail",
                "InfoBaseBrush");
        }

        OnPropertyChanged(nameof(IsFiveMSessionActive));
        OnPropertyChanged(nameof(IsFiveMSessionEndConfirmationPending));
        OnPropertyChanged(nameof(IsFiveMSessionReadUnavailable));
        OnPropertyChanged(nameof(CanToggleFiveMSessionMonitor));
        RaiseCommandState();
    }

    private void SetFiveMSessionPresentation(string statusKey, string detailKey, string brushKey)
    {
        FiveMSessionStatusLabel = localization.GetString(statusKey);
        FiveMSessionDetailLabel = localization.GetString(detailKey);
        FiveMSessionStatusBrushKey = brushKey;
    }

    private bool HasLegacySessionRoot() => diagnostic is
    {
        Edition: FiveMEdition.Legacy,
        FiveMRoot: not null
    } && !string.IsNullOrWhiteSpace(diagnostic.FiveMRoot);

    private static string FormatSessionDuration(TimeSpan duration)
    {
        var normalized = duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{(int)normalized.TotalHours:00}:{normalized.Minutes:00}:{normalized.Seconds:00}");
    }
}
