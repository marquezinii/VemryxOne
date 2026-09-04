using System.Collections.ObjectModel;
using Ralven.App.Services;
using Ralven.Windows.Infrastructure;

namespace Ralven.App.ViewModels;

internal sealed record InstalledApplicationDisplayItem(
    string Name,
    string Publisher,
    string Version,
    string EstimatedSize,
    string Scope);

internal sealed record StartupApplicationDisplayItem(
    string Name,
    string Source,
    string Scope);

internal sealed record ApplicationUpdateDisplayItem(
    WindowsApplicationUpdate Update,
    string Name,
    string PackageId,
    string InstalledVersion,
    string AvailableVersion,
    string Source,
    string ActionAutomationName);

internal sealed class ApplicationsPageViewModel : BindableBase, IDisposable
{
    private readonly IWindowsApplicationInventoryInspector inspector;
    private readonly IWindowsApplicationUpdateService updateService;
    private readonly ILocalizationService localization;
    private IReadOnlyList<WindowsInstalledApplication> installedApplications = [];
    private IReadOnlyList<WindowsStartupItem> startupItems = [];
    private IReadOnlyList<WindowsApplicationUpdate> applicationUpdates = [];
    private WindowsApplicationInventorySnapshot? snapshot;
    private WindowsApplicationUpdateSnapshot? updateSnapshot;
    private string searchText = string.Empty;
    private string inventoryStatusMessage;
    private string inventoryObservedAtLabel = string.Empty;
    private string applicationUpdateStatusMessage;
    private string applicationUpdatesObservedAtLabel = string.Empty;
    private bool isInventoryLoading;
    private bool isCheckingApplicationUpdates;
    private bool isUpdatingApplication;
    private bool inventoryUnavailable;
    private bool applicationUpdatesUnavailable;
    private bool winGetUnavailable;
    private bool disposed;

    public ApplicationsPageViewModel(
        IWindowsApplicationInventoryInspector inspector,
        ILocalizationService? localization = null)
        : this(inspector, new WinGetApplicationUpdateService(), localization)
    {
    }

    public ApplicationsPageViewModel(
        IWindowsApplicationInventoryInspector inspector,
        IWindowsApplicationUpdateService updateService,
        ILocalizationService? localization = null)
    {
        this.inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
        this.updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));
        this.localization = localization ?? LocalizationService.Current;
        inventoryStatusMessage = this.localization.GetString(
            "Applications.Inventory.Status.Loading");
        applicationUpdateStatusMessage = this.localization.GetString(
            "Applications.Updates.Status.Checking");
        this.localization.LanguageChanged += Localization_LanguageChanged;
    }

    public ObservableCollection<InstalledApplicationDisplayItem> InstalledApplications { get; } = [];

    public ObservableCollection<StartupApplicationDisplayItem> StartupItems { get; } = [];

    public ObservableCollection<ApplicationUpdateDisplayItem> ApplicationUpdates { get; } = [];

    public int InstalledApplicationCount => installedApplications.Count;

    public int StartupItemCount => startupItems.Count;

    public int ApplicationUpdateCount => applicationUpdates.Count;

    public bool IsInventoryLoading => isInventoryLoading;

    public bool CanRefreshInventory => !isInventoryLoading;

    public bool CanRefreshApplications => !isInventoryLoading
        && !isCheckingApplicationUpdates
        && !isUpdatingApplication;

    public bool CanUpdateApplications => !isCheckingApplicationUpdates
        && !isUpdatingApplication;

    public bool HasInstalledApplications => InstalledApplications.Count > 0;

    public bool HasStartupItems => StartupItems.Count > 0;

    public bool HasApplicationUpdates => ApplicationUpdates.Count > 0;

    public bool ShowInstalledEmptyState => (snapshot is not null || inventoryUnavailable)
        && !isInventoryLoading
        && !HasInstalledApplications;

    public bool ShowStartupEmptyState => (snapshot is not null || inventoryUnavailable)
        && !isInventoryLoading
        && !HasStartupItems;

    public bool ShowApplicationUpdatesEmptyState => (updateSnapshot is not null
            || applicationUpdatesUnavailable)
        && !isCheckingApplicationUpdates
        && !HasApplicationUpdates;

    public string InventoryStatusMessage
    {
        get => inventoryStatusMessage;
        private set => SetProperty(ref inventoryStatusMessage, value);
    }

    public string InventoryObservedAtLabel
    {
        get => inventoryObservedAtLabel;
        private set => SetProperty(ref inventoryObservedAtLabel, value);
    }

    public string ApplicationUpdateStatusMessage
    {
        get => applicationUpdateStatusMessage;
        private set => SetProperty(ref applicationUpdateStatusMessage, value);
    }

    public string ApplicationUpdatesObservedAtLabel
    {
        get => applicationUpdatesObservedAtLabel;
        private set => SetProperty(ref applicationUpdatesObservedAtLabel, value);
    }

    public string InstalledEmptyMessage => GetEmptyMessage(
        snapshot?.InstalledApplicationsComplete,
        installedApplications.Count > 0,
        "Applications.Inventory.Empty.Installed",
        "Applications.Inventory.NoMatches.Installed",
        "Applications.Inventory.Incomplete.Installed",
        "Applications.Inventory.Unavailable.Installed");

    public string StartupEmptyMessage => GetEmptyMessage(
        snapshot?.StartupItemsComplete,
        startupItems.Count > 0,
        "Applications.Inventory.Empty.Startup",
        "Applications.Inventory.NoMatches.Startup",
        "Applications.Inventory.Incomplete.Startup",
        "Applications.Inventory.Unavailable.Startup");

    public string ApplicationUpdatesEmptyMessage => applicationUpdatesUnavailable
        ? localization.GetString("Applications.Updates.Empty.Unavailable")
        : winGetUnavailable
            ? localization.GetString("Applications.Updates.Empty.WinGetUnavailable")
            : applicationUpdates.Count > 0
                ? localization.GetString("Applications.Updates.Empty.NoMatches")
                : localization.GetString("Applications.Updates.Empty.Current");

    public string SearchText
    {
        get => searchText;
        set
        {
            if (SetProperty(ref searchText, value ?? string.Empty))
            {
                ApplyFilter();
            }
        }
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (isInventoryLoading)
        {
            return;
        }

        SetLoading(true);
        InventoryStatusMessage = localization.GetString(
            "Applications.Inventory.Status.Loading");
        try
        {
            snapshot = await inspector.InspectAsync(cancellationToken);
            inventoryUnavailable = false;
            installedApplications = snapshot.InstalledApplications;
            startupItems = snapshot.StartupItems;
            ApplySnapshotPresentation();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception) when (exception is not (
            OutOfMemoryException or StackOverflowException or AccessViolationException))
        {
            inventoryUnavailable = true;
            InventoryStatusMessage = localization.GetString(
                "Applications.Inventory.Status.Unavailable");
            if (snapshot is null)
            {
                installedApplications = [];
                startupItems = [];
                ReplaceWith(InstalledApplications, []);
                ReplaceWith(StartupItems, []);
            }

            NotifyInventoryCountsAndVisibility();
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task CheckApplicationUpdatesAsync(
        CancellationToken cancellationToken = default)
    {
        if (isCheckingApplicationUpdates || isUpdatingApplication)
        {
            return;
        }

        SetCheckingApplicationUpdates(true);
        ApplicationUpdateStatusMessage = localization.GetString(
            "Applications.Updates.Status.Checking");
        try
        {
            updateSnapshot = await updateService.CheckAsync(cancellationToken);
            applicationUpdatesUnavailable = false;
            winGetUnavailable = !updateSnapshot.IsWinGetAvailable;
            applicationUpdates = updateSnapshot.Updates;
            ApplyApplicationUpdatePresentation();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception) when (exception is not (
            OutOfMemoryException or StackOverflowException or AccessViolationException))
        {
            updateSnapshot = null;
            applicationUpdates = [];
            applicationUpdatesUnavailable = true;
            winGetUnavailable = false;
            ApplicationUpdatesObservedAtLabel = string.Empty;
            ApplicationUpdateStatusMessage = localization.GetString(
                "Applications.Updates.Status.Unavailable");
            ApplyFilter();
        }
        finally
        {
            SetCheckingApplicationUpdates(false);
        }
    }

    public async Task UpdateApplicationAsync(
        ApplicationUpdateDisplayItem item,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        var current = applicationUpdates.FirstOrDefault(update =>
            string.Equals(update.PackageId, item.PackageId, StringComparison.Ordinal)
            && string.Equals(update.AvailableVersion, item.AvailableVersion, StringComparison.Ordinal));
        if (isUpdatingApplication || current is null)
        {
            return;
        }

        SetUpdatingApplication(true);
        ApplicationUpdateStatusMessage = localization.Format(
            "Applications.Updates.Status.Updating",
            current.Name);
        try
        {
            var result = await updateService.UpdateAsync(current, cancellationToken);
            if (result.Outcome is WindowsApplicationUpdateOutcome.Succeeded
                or WindowsApplicationUpdateOutcome.NoLongerAvailable)
            {
                applicationUpdates = applicationUpdates
                    .Where(update => !string.Equals(
                        update.PackageId,
                        current.PackageId,
                        StringComparison.Ordinal))
                    .ToArray();
                updateSnapshot = updateSnapshot is null
                    ? null
                    : updateSnapshot with { Updates = applicationUpdates };
                ApplicationUpdateStatusMessage = localization.Format(
                    result.Outcome == WindowsApplicationUpdateOutcome.Succeeded
                        ? "Applications.Updates.Status.Succeeded"
                        : "Applications.Updates.Status.NoLongerAvailable",
                    current.Name);
                ApplyFilter();
                return;
            }

            ApplicationUpdateStatusMessage = result.Outcome
                == WindowsApplicationUpdateOutcome.WinGetUnavailable
                ? localization.GetString("Applications.Updates.Status.WinGetUnavailable")
                : localization.Format(
                    "Applications.Updates.Status.Failed",
                    current.Name,
                    FormatExitCode(result.ExitCode));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception) when (exception is not (
            OutOfMemoryException or StackOverflowException or AccessViolationException))
        {
            ApplicationUpdateStatusMessage = localization.Format(
                "Applications.Updates.Status.Failed",
                current.Name,
                localization.GetString("Common.Unknown"));
        }
        finally
        {
            SetUpdatingApplication(false);
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        localization.LanguageChanged -= Localization_LanguageChanged;
    }

    private void ApplySnapshotPresentation()
    {
        var currentSnapshot = snapshot!;
        InventoryStatusMessage = localization.GetString(currentSnapshot.IsPartial
            ? "Applications.Inventory.Status.Partial"
            : "Applications.Inventory.Status.Ready");
        InventoryObservedAtLabel = localization.Format(
            "Applications.Inventory.Status.UpdatedAt",
            currentSnapshot.ObservedAtUtc.ToLocalTime().ToString("t", localization.CurrentCulture));
        ApplyFilter();
    }

    private void ApplyApplicationUpdatePresentation()
    {
        var currentSnapshot = updateSnapshot!;
        ApplicationUpdateStatusMessage = localization.GetString(
            currentSnapshot.IsWinGetAvailable
                ? currentSnapshot.Updates.Count > 0
                    ? "Applications.Updates.Status.Available"
                    : "Applications.Updates.Status.Current"
                : "Applications.Updates.Status.WinGetUnavailable");
        ApplicationUpdatesObservedAtLabel = currentSnapshot.IsWinGetAvailable
            ? localization.Format(
                "Applications.Inventory.Status.UpdatedAt",
                currentSnapshot.ObservedAtUtc.ToLocalTime().ToString(
                    "t",
                    localization.CurrentCulture))
            : string.Empty;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = searchText.Trim();
        var installed = installedApplications
            .Where(application => MatchesInstalled(application, query))
            .Select(CreateDisplayItem)
            .ToArray();
        var startup = startupItems
            .Where(item => Matches(item.Name, query))
            .Select(CreateDisplayItem)
            .ToArray();
        var updates = applicationUpdates
            .Where(update => MatchesUpdate(update, query))
            .Select(CreateDisplayItem)
            .ToArray();

        ReplaceWith(InstalledApplications, installed);
        ReplaceWith(StartupItems, startup);
        ReplaceWith(ApplicationUpdates, updates);
        NotifyInventoryCountsAndVisibility();
        NotifyApplicationUpdateCountsAndVisibility();
    }

    private string GetEmptyMessage(
        bool? inventoryComplete,
        bool hasUnfilteredItems,
        string emptyKey,
        string noMatchesKey,
        string incompleteKey,
        string unavailableKey)
    {
        if (inventoryUnavailable)
        {
            return localization.GetString(unavailableKey);
        }

        var key = inventoryComplete switch
        {
            null => unavailableKey,
            false => incompleteKey,
            _ when hasUnfilteredItems => noMatchesKey,
            _ => emptyKey
        };
        return localization.GetString(key);
    }

    private InstalledApplicationDisplayItem CreateDisplayItem(
        WindowsInstalledApplication application)
    {
        return new InstalledApplicationDisplayItem(
            application.DisplayName,
            ValueOrUnknown(application.Publisher),
            ValueOrUnknown(application.DisplayVersion),
            FormatSize(application.EstimatedSizeBytes),
            DescribeScope(application.Scope));
    }

    private StartupApplicationDisplayItem CreateDisplayItem(WindowsStartupItem item)
    {
        return new StartupApplicationDisplayItem(
            item.Name,
            localization.GetString(item.Source switch
            {
                WindowsStartupItemSource.RegistryRun =>
                    "Applications.Inventory.Source.RegistryRun",
                WindowsStartupItemSource.RegistryRunOnce =>
                    "Applications.Inventory.Source.RegistryRunOnce",
                _ => "Applications.Inventory.Source.StartupFolder"
            }),
            DescribeScope(item.Scope));
    }

    private ApplicationUpdateDisplayItem CreateDisplayItem(
        WindowsApplicationUpdate update)
    {
        return new ApplicationUpdateDisplayItem(
            update,
            update.Name,
            update.PackageId,
            update.InstalledVersion,
            update.AvailableVersion,
            update.Source,
            localization.Format("Applications.Updates.ActionFor", update.Name));
    }

    private string DescribeScope(WindowsApplicationScope scope)
    {
        return localization.GetString(scope == WindowsApplicationScope.LocalMachine
            ? "Applications.Inventory.Scope.LocalMachine"
            : "Applications.Inventory.Scope.CurrentUser");
    }

    private string ValueOrUnknown(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? localization.GetString("Common.Unknown")
            : value;
    }

    private string FormatSize(long? bytes)
    {
        if (bytes is not > 0)
        {
            return localization.GetString("Common.Unknown");
        }

        const double kibibyte = 1024d;
        const double mebibyte = 1024d * kibibyte;
        const double gibibyte = 1024d * mebibyte;
        return bytes.Value switch
        {
            >= (long)gibibyte => $"{(bytes.Value / gibibyte).ToString("N1", localization.CurrentCulture)} GB",
            >= (long)mebibyte => $"{(bytes.Value / mebibyte).ToString("N0", localization.CurrentCulture)} MB",
            _ => $"{(bytes.Value / kibibyte).ToString("N0", localization.CurrentCulture)} KB"
        };
    }

    private static bool MatchesInstalled(
        WindowsInstalledApplication application,
        string query)
    {
        return Matches(application.DisplayName, query)
            || Matches(application.Publisher, query)
            || Matches(application.DisplayVersion, query);
    }

    private static bool MatchesUpdate(WindowsApplicationUpdate update, string query)
    {
        return Matches(update.Name, query)
            || Matches(update.PackageId, query)
            || Matches(update.InstalledVersion, query)
            || Matches(update.AvailableVersion, query);
    }

    private static bool Matches(string? value, string query)
    {
        return query.Length == 0
            || value?.Contains(query, StringComparison.OrdinalIgnoreCase) == true;
    }

    private static void ReplaceWith<T>(
        ObservableCollection<T> target,
        IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private void SetLoading(bool value)
    {
        if (isInventoryLoading == value)
        {
            return;
        }

        isInventoryLoading = value;
        OnPropertyChanged(nameof(IsInventoryLoading));
        OnPropertyChanged(nameof(CanRefreshInventory));
        OnPropertyChanged(nameof(CanRefreshApplications));
        OnPropertyChanged(nameof(ShowInstalledEmptyState));
        OnPropertyChanged(nameof(ShowStartupEmptyState));
        OnPropertyChanged(nameof(InstalledEmptyMessage));
        OnPropertyChanged(nameof(StartupEmptyMessage));
    }

    private void SetCheckingApplicationUpdates(bool value)
    {
        if (isCheckingApplicationUpdates == value)
        {
            return;
        }

        isCheckingApplicationUpdates = value;
        OnPropertyChanged(nameof(CanRefreshApplications));
        OnPropertyChanged(nameof(CanUpdateApplications));
        OnPropertyChanged(nameof(ShowApplicationUpdatesEmptyState));
        OnPropertyChanged(nameof(ApplicationUpdatesEmptyMessage));
    }

    private void SetUpdatingApplication(bool value)
    {
        if (isUpdatingApplication == value)
        {
            return;
        }

        isUpdatingApplication = value;
        OnPropertyChanged(nameof(CanRefreshApplications));
        OnPropertyChanged(nameof(CanUpdateApplications));
    }

    private void NotifyInventoryCountsAndVisibility()
    {
        OnPropertyChanged(nameof(InstalledApplicationCount));
        OnPropertyChanged(nameof(StartupItemCount));
        OnPropertyChanged(nameof(HasInstalledApplications));
        OnPropertyChanged(nameof(HasStartupItems));
        OnPropertyChanged(nameof(ShowInstalledEmptyState));
        OnPropertyChanged(nameof(ShowStartupEmptyState));
        OnPropertyChanged(nameof(InstalledEmptyMessage));
        OnPropertyChanged(nameof(StartupEmptyMessage));
    }

    private void NotifyApplicationUpdateCountsAndVisibility()
    {
        OnPropertyChanged(nameof(ApplicationUpdateCount));
        OnPropertyChanged(nameof(HasApplicationUpdates));
        OnPropertyChanged(nameof(ShowApplicationUpdatesEmptyState));
        OnPropertyChanged(nameof(ApplicationUpdatesEmptyMessage));
    }

    private string FormatExitCode(int? exitCode) => exitCode is null
        ? localization.GetString("Common.Unknown")
        : $"0x{exitCode.Value:X8}";

    private void Localization_LanguageChanged(object? sender, AppLanguageChangedEventArgs e)
    {
        OnPropertyChanged(nameof(InstalledEmptyMessage));
        OnPropertyChanged(nameof(StartupEmptyMessage));
        OnPropertyChanged(nameof(ApplicationUpdatesEmptyMessage));
        if (inventoryUnavailable)
        {
            InventoryStatusMessage = localization.GetString(
                "Applications.Inventory.Status.Unavailable");
        }
        else if (snapshot is not null)
        {
            ApplySnapshotPresentation();
        }
        else
        {
            InventoryStatusMessage = localization.GetString(
                inventoryUnavailable
                    ? "Applications.Inventory.Status.Unavailable"
                    : "Applications.Inventory.Status.Loading");
        }

        if (isUpdatingApplication)
        {
            return;
        }

        if (applicationUpdatesUnavailable)
        {
            ApplicationUpdateStatusMessage = localization.GetString(
                "Applications.Updates.Status.Unavailable");
        }
        else if (updateSnapshot is not null)
        {
            ApplyApplicationUpdatePresentation();
        }
        else
        {
            ApplicationUpdateStatusMessage = localization.GetString(
                "Applications.Updates.Status.Checking");
        }
    }
}

internal sealed class SyntheticWindowsApplicationInventoryInspector
    : IWindowsApplicationInventoryInspector
{
    public Task<WindowsApplicationInventorySnapshot> InspectAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new WindowsApplicationInventorySnapshot(
            [
                new WindowsInstalledApplication(
                    "FiveM",
                    "1.0",
                    "Cfx.re",
                    null,
                    WindowsApplicationScope.CurrentUser,
                    WindowsApplicationArchitecture.X64),
                new WindowsInstalledApplication(
                    "OBS Studio",
                    "32.0",
                    "OBS Project",
                    null,
                    WindowsApplicationScope.LocalMachine,
                    WindowsApplicationArchitecture.X64),
                new WindowsInstalledApplication(
                    "Ralven",
                    "1.5.1",
                    null,
                    null,
                    WindowsApplicationScope.CurrentUser,
                    WindowsApplicationArchitecture.X64)
            ],
            [
                new WindowsStartupItem(
                    "Ralven",
                    "CurrentUser:RegistryRun",
                    WindowsStartupItemSource.RegistryRun,
                    WindowsApplicationScope.CurrentUser),
                new WindowsStartupItem(
                    "Game launcher",
                    "CurrentUser:StartupFolder",
                    WindowsStartupItemSource.StartupFolder,
                    WindowsApplicationScope.CurrentUser)
            ],
            DateTimeOffset.UtcNow,
            InstalledApplicationsComplete: true,
            StartupItemsComplete: true));
    }

    public Task<WindowsApplicationInventorySnapshot> InspectStartupAsync(
        CancellationToken cancellationToken = default) => InspectAsync(cancellationToken);
}

internal sealed class SyntheticWindowsApplicationUpdateService
    : IWindowsApplicationUpdateService
{
    public Task<WindowsApplicationUpdateSnapshot> CheckAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new WindowsApplicationUpdateSnapshot(
            [
                new WindowsApplicationUpdate(
                    "OBSProject.OBSStudio",
                    "OBS Studio",
                    "32.0",
                    "32.1.2",
                    "winget"),
                new WindowsApplicationUpdate(
                    "VideoLAN.VLC",
                    "VLC media player",
                    "3.0.21",
                    "3.0.22",
                    "winget")
            ],
            DateTimeOffset.UtcNow,
            IsWinGetAvailable: true));
    }

    public Task<WindowsApplicationUpdateResult> UpdateAsync(
        WindowsApplicationUpdate update,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new WindowsApplicationUpdateResult(
            WindowsApplicationUpdateOutcome.Succeeded,
            ExitCode: 0));
    }
}
