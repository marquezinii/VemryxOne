using Ralven.Contracts;
using Ralven.Core.Catalog;
using Ralven.Windows.Infrastructure;

namespace Ralven.Windows.Actions;

public sealed class WindowsSecurityHealthDiagnosisAction : AsyncReadOnlyDiagnosticAction
{
    private readonly IWindowsSystemHealthInspector inspector;
    private readonly WindowsActionTextResolver text;

    public WindowsSecurityHealthDiagnosisAction(
        IWindowsSystemHealthInspector inspector,
        WindowsActionTextResolver text)
    {
        this.inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
        this.text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public override ActionMetadataDto Metadata { get; } = WindowsActionMetadata.For(
        OptimizationActionIds.DiagnoseWindowsSecurityHealth);

    protected override async Task<string> DescribeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await inspector.InspectAsync(cancellationToken).ConfigureAwait(false);
            return Classify(snapshot, text);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is not (
            OutOfMemoryException or StackOverflowException or AccessViolationException))
        {
            return text("ActionResults.WindowsSecurity.Unavailable");
        }
    }

    internal static string Classify(
        WindowsSystemHealthSnapshot snapshot,
        WindowsActionTextResolver text)
    {
        if (snapshot.Antivirus.State == WindowsSecurityHealthState.Good
            && snapshot.Firewall.State == WindowsSecurityHealthState.Good
            && snapshot.AutomaticUpdates.State == WindowsSecurityHealthState.Good)
        {
            return text("ActionResults.WindowsSecurity.AllHealthy");
        }

        var status = text(
            "ActionResults.WindowsSecurity.Summary",
            DescribeState(snapshot.Antivirus.State, text),
            DescribeState(snapshot.Firewall.State, text),
            DescribeState(snapshot.AutomaticUpdates.State, text));
        var availability = snapshot.IsPartial
            ? text("ActionResults.WindowsSecurity.PartialSuffix")
            : string.Empty;

        return status + availability;
    }

    private static string DescribeState(
        WindowsSecurityHealthState state,
        WindowsActionTextResolver text) => text(state switch
        {
            WindowsSecurityHealthState.Good => "ActionResults.WindowsSecurity.State.Good",
            WindowsSecurityHealthState.NotMonitored => "ActionResults.WindowsSecurity.State.NotMonitored",
            WindowsSecurityHealthState.Poor => "ActionResults.WindowsSecurity.State.Poor",
            WindowsSecurityHealthState.Snoozed => "ActionResults.WindowsSecurity.State.Snoozed",
            _ => "ActionResults.WindowsSecurity.State.Unavailable"
        });
}

public sealed class StartupLoadDiagnosisAction : AsyncReadOnlyDiagnosticAction
{
    private readonly IWindowsApplicationInventoryInspector inspector;
    private readonly WindowsActionTextResolver text;

    public StartupLoadDiagnosisAction(
        IWindowsApplicationInventoryInspector inspector,
        WindowsActionTextResolver text)
    {
        this.inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
        this.text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public override ActionMetadataDto Metadata { get; } = WindowsActionMetadata.For(
        OptimizationActionIds.DiagnoseStartupLoad);

    protected override async Task<string> DescribeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await inspector.InspectStartupAsync(cancellationToken).ConfigureAwait(false);
            return Classify(snapshot, text);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is not (
            OutOfMemoryException or StackOverflowException or AccessViolationException))
        {
            return text("ActionResults.StartupLoad.Unavailable");
        }
    }

    internal static string Classify(
        WindowsApplicationInventorySnapshot snapshot,
        WindowsActionTextResolver text)
    {
        var itemCount = snapshot.StartupItems.Count;
        var partial = snapshot.StartupItemsComplete
            ? string.Empty
            : text("ActionResults.StartupLoad.PartialSuffix");

        if (itemCount == 0)
        {
            return text(snapshot.StartupItemsComplete
                ? "ActionResults.StartupLoad.NoItems"
                : "ActionResults.StartupLoad.NoItemsAccessible") + partial;
        }

        var registryCount = snapshot.StartupItems.Count(static item =>
            item.Source is WindowsStartupItemSource.RegistryRun or WindowsStartupItemSource.RegistryRunOnce);
        var folderCount = itemCount - registryCount;

        return text(
            "ActionResults.StartupLoad.Summary",
            itemCount,
            registryCount,
            folderCount) + partial;
    }
}

public sealed class TrimStatusDiagnosisAction : AsyncReadOnlyDiagnosticAction
{
    private readonly ITrimStatusInspector inspector;
    private readonly WindowsActionTextResolver text;

    public TrimStatusDiagnosisAction(
        ITrimStatusInspector inspector,
        WindowsActionTextResolver text)
    {
        this.inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
        this.text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public override ActionMetadataDto Metadata { get; } = WindowsActionMetadata.For(
        OptimizationActionIds.DiagnoseTrimStatus);

    protected override async Task<string> DescribeAsync(CancellationToken cancellationToken)
    {
        var snapshot = await inspector.InspectAsync(cancellationToken).ConfigureAwait(false);
        return snapshot.State switch
        {
            TrimInspectionState.AccessDenied => text("ActionResults.TrimStatus.AccessDenied"),
            TrimInspectionState.Unsupported => text("ActionResults.TrimStatus.Unsupported"),
            TrimInspectionState.Unavailable => text("ActionResults.TrimStatus.Unavailable"),
            _ => DescribeAvailable(snapshot, text)
        };
    }

    internal static string DescribeAvailable(
        TrimStatusSnapshot snapshot,
        WindowsActionTextResolver text)
    {
        var states = new List<string>(2);
        if (snapshot.Ntfs is { } ntfs)
        {
            states.Add(text("ActionResults.TrimStatus.FileSystemState", "NTFS", Describe(ntfs, text)));
        }

        if (snapshot.ReFs is { } refs)
        {
            states.Add(text("ActionResults.TrimStatus.FileSystemState", "ReFS", Describe(refs, text)));
        }

        if (states.Count == 0)
        {
            return text("ActionResults.TrimStatus.Unavailable");
        }

        var partial = snapshot.State == TrimInspectionState.Partial
            ? text("ActionResults.TrimStatus.PartialSuffix")
            : string.Empty;
        return text("ActionResults.TrimStatus.Summary", string.Join("; ", states)) + partial;
    }

    private static string Describe(
        TrimDeleteNotificationState state,
        WindowsActionTextResolver text) => text(state switch
        {
            TrimDeleteNotificationState.Enabled => "ActionResults.TrimStatus.State.Enabled",
            TrimDeleteNotificationState.Disabled => "ActionResults.TrimStatus.State.Disabled",
            _ => "ActionResults.TrimStatus.State.Unknown"
        });
}

public sealed class MouseAccelerationDiagnosisAction : ReadOnlyDiagnosticAction
{
    private readonly IMouseAccelerationInspector inspector;
    private readonly WindowsActionTextResolver text;

    public MouseAccelerationDiagnosisAction(
        IMouseAccelerationInspector inspector,
        WindowsActionTextResolver text)
    {
        this.inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
        this.text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public override ActionMetadataDto Metadata { get; } = WindowsActionMetadata.For(
        OptimizationActionIds.DiagnoseMouseAcceleration);

    protected override string Describe()
    {
        var snapshot = inspector.GetSnapshot();
        if (snapshot.State != MouseAccelerationInspectionState.Available
            || snapshot.Threshold1 is not { } threshold1
            || snapshot.Threshold2 is not { } threshold2
            || snapshot.AccelerationLevel is not { } level)
        {
            return text("ActionResults.MouseAcceleration.Unavailable");
        }

        return level > 0
            ? text("ActionResults.MouseAcceleration.Active", level, threshold1, threshold2)
            : text("ActionResults.MouseAcceleration.Inactive", threshold1, threshold2);
    }
}
