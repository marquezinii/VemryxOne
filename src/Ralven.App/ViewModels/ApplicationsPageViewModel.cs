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

internal sealed class ApplicationsPageViewModel : BindableBase, IDisposable
{
    private readonly IWindowsApplicationInventoryInspector inspector;
    private readonly ILocalizationService localization;
    private IReadOnlyList<WindowsInstalledApplication> installedApplications = [];
    private IReadOnlyList<WindowsStartupItem> startupItems = [];
    private WindowsApplicationInventorySnapshot? snapshot;
    private string searchText = string.Empty;
    private string inventoryStatusMessage;
    private string inventoryObservedAtLabel = string.Empty;
    private bool isInventoryLoading;
    private bool inventoryUnavailable;
    private bool disposed;

    public ApplicationsPageViewModel(
        IWindowsApplicationInventoryInspector inspector,
        ILocalizationService? localization = null)
    {
        this.inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
        this.localization = localization ?? LocalizationService.Current;
        inventoryStatusMessage = this.localization.GetString(
            "Applications.Inventory.Status.Loading");
        this.localization.LanguageChanged += Localization_LanguageChanged;
    }

    public ObservableCollection<InstalledApplicationDisplayItem> InstalledApplications { get; } = [];

    public ObservableCollection<StartupApplicationDisplayItem> StartupItems { get; } = [];

    public int InstalledApplicationCount => installedApplications.Count;

    public int StartupItemCount => startupItems.Count;

    public bool IsInventoryLoading => isInventoryLoading;

    public bool CanRefreshInventory => !isInventoryLoading;

    public bool HasInstalledApplications => InstalledApplications.Count > 0;

    public bool HasStartupItems => StartupItems.Count > 0;

    public bool ShowInstalledEmptyState => (snapshot is not null || inventoryUnavailable)
        && !isInventoryLoading
        && !HasInstalledApplications;

    public bool ShowStartupEmptyState => (snapshot is not null || inventoryUnavailable)
        && !isInventoryLoading
        && !HasStartupItems;

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

        ReplaceWith(InstalledApplications, installed);
        ReplaceWith(StartupItems, startup);
        NotifyInventoryCountsAndVisibility();
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
        OnPropertyChanged(nameof(ShowInstalledEmptyState));
        OnPropertyChanged(nameof(ShowStartupEmptyState));
        OnPropertyChanged(nameof(InstalledEmptyMessage));
        OnPropertyChanged(nameof(StartupEmptyMessage));
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

    private void Localization_LanguageChanged(object? sender, AppLanguageChangedEventArgs e)
    {
        OnPropertyChanged(nameof(InstalledEmptyMessage));
        OnPropertyChanged(nameof(StartupEmptyMessage));
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
