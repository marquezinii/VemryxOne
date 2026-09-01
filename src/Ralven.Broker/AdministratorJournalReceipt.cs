using System.Text.Json;
using Microsoft.Win32;
using Ralven.Contracts;
using Ralven.Windows.Engine;

namespace Ralven.Broker;

internal interface IAdministratorJournalReceiptStore
{
    void Seal(Guid transactionId, byte[] payload);

    byte[]? Read(Guid transactionId);
}

internal sealed class RegistryAdministratorJournalReceiptStore : IAdministratorJournalReceiptStore
{
    // Receipts remain after rollback so a replayed local journal cannot regain
    // authority and callers can safely retry after losing the terminal event.
    internal const int MaximumPayloadBytes = 512 * 1024;
    public void Seal(Guid transactionId, byte[] payload)
    {
        Validate(transactionId, payload);
        using var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var key = localMachine.CreateSubKey(
            ProductIdentity.AdministratorReceiptRegistryPath,
            writable: true)
            ?? throw new UnauthorizedAccessException("The broker receipt registry key is unavailable.");
        key.SetValue(transactionId.ToString("N"), payload, RegistryValueKind.Binary);
    }

    public byte[]? Read(Guid transactionId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(transactionId, Guid.Empty);
        using var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var key = localMachine.OpenSubKey(
            ProductIdentity.AdministratorReceiptRegistryPath,
            writable: false);
        if (key is null)
        {
            return null;
        }

        var name = transactionId.ToString("N");
        var value = key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
        if (value is null)
        {
            return null;
        }

        if (key.GetValueKind(name) != RegistryValueKind.Binary || value is not byte[] payload)
        {
            throw new InvalidDataException("The protected broker receipt has an invalid registry type.");
        }

        Validate(transactionId, payload);
        return payload;
    }

    private static void Validate(Guid transactionId, byte[] payload)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(transactionId, Guid.Empty);
        ArgumentNullException.ThrowIfNull(payload);
        if (payload.Length is 0 or > MaximumPayloadBytes)
        {
            throw new InvalidDataException("The protected broker receipt has an invalid size.");
        }
    }
}

internal sealed class BrokerAdministratorJournalStore : IWindowsTransactionJournalStore
{
    private readonly IWindowsTransactionJournalStore inner;
    private readonly IAdministratorJournalReceiptStore receipts;
    private readonly bool requireReceiptOnLoad;

    public BrokerAdministratorJournalStore(
        IWindowsTransactionJournalStore inner,
        IAdministratorJournalReceiptStore receipts,
        bool requireReceiptOnLoad)
    {
        this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        this.receipts = receipts ?? throw new ArgumentNullException(nameof(receipts));
        this.requireReceiptOnLoad = requireReceiptOnLoad;
    }

    public async Task SaveAsync(
        WindowsTransactionJournal journal,
        CancellationToken cancellationToken)
    {
        var payload = AdministratorJournalReceipt.Serialize(journal);
        receipts.Seal(journal.TransactionId, payload);
        await inner.SaveAsync(journal, cancellationToken).ConfigureAwait(false);
    }

    public async Task<WindowsTransactionJournal?> LoadAsync(
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        var journal = await inner.LoadAsync(transactionId, cancellationToken).ConfigureAwait(false);
        var protectedPayload = receipts.Read(transactionId);
        if (protectedPayload is not null)
        {
            if (journal is null)
            {
                throw new InvalidDataException(
                    "The protected broker receipt exists but the transaction journal is missing.");
            }

            if (AdministratorJournalReceipt.Restore(journal, protectedPayload))
            {
                await inner.SaveAsync(journal, cancellationToken).ConfigureAwait(false);
            }

            return journal;
        }

        if (journal is null)
        {
            return null;
        }

        if (!requireReceiptOnLoad && journal.Actions
            .Where(action => action.RequiredPrivilege == RequiredPrivilege.Administrator)
            .All(IsPristineAdministratorEntry))
        {
            return journal;
        }

        throw new UnauthorizedAccessException(
            requireReceiptOnLoad
                ? "The administrator transaction journal does not have a broker receipt."
                : "The administrator transaction journal contains untrusted execution state.");

        static bool IsPristineAdministratorEntry(WindowsActionJournalEntry entry)
        {
            return (entry.State is ActionJournalState.Pending
                    or ActionJournalState.DeferredPrivilege
                    or ActionJournalState.SkippedPrivilege)
                && !entry.Changed
                && string.IsNullOrWhiteSpace(entry.SnapshotJson)
                && !entry.RollbackSafeAfterInterruption
                && entry.Outcome == ActionExecutionOutcome.Pending;
        }
    }
}

internal static class AdministratorJournalReceipt
{
    private const int ReceiptFormatVersion = 1;

    public static byte[] Serialize(WindowsTransactionJournal journal)
    {
        ArgumentNullException.ThrowIfNull(journal);
        var actions = journal.Actions
            .Where(action => action.RequiredPrivilege == RequiredPrivilege.Administrator)
            .OrderBy(action => action.Sequence)
            .Select(action => new ReceiptAction(
                action.Sequence,
                action.ActionId,
                action.Version,
                action.RequiredPrivilege,
                action.Reversibility,
                action.State,
                action.Outcome,
                action.Changed,
                action.SnapshotJson,
                action.RollbackSafeAfterInterruption))
            .ToArray();
        if (actions.Length == 0)
        {
            throw new InvalidDataException("The journal contains no administrator actions.");
        }

        var payload = JsonSerializer.SerializeToUtf8Bytes(
            new ReceiptPayload(
                ReceiptFormatVersion,
                journal.TransactionId,
                journal.SchemaVersion,
                journal.WasElevated,
                journal.Profile,
                actions),
            RalvenJson.Options);
        if (payload.Length > RegistryAdministratorJournalReceiptStore.MaximumPayloadBytes)
        {
            throw new InvalidDataException("The administrator receipt exceeds the supported size.");
        }

        return payload;
    }

    public static bool Restore(WindowsTransactionJournal journal, byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(payload);
        if (payload.Length is 0 or > RegistryAdministratorJournalReceiptStore.MaximumPayloadBytes)
        {
            throw new InvalidDataException("The protected broker receipt has an invalid size.");
        }

        ReceiptPayload receipt;
        try
        {
            receipt = JsonSerializer.Deserialize<ReceiptPayload>(payload, RalvenJson.Options)
                ?? throw new InvalidDataException("The protected broker receipt is empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("The protected broker receipt is corrupt.", exception);
        }

        if (receipt.FormatVersion != ReceiptFormatVersion
            || receipt.TransactionId != journal.TransactionId
            || receipt.SchemaVersion != journal.SchemaVersion
            || receipt.Actions is null
            || receipt.Actions.Count == 0)
        {
            throw new InvalidDataException("The protected broker receipt metadata is invalid.");
        }

        var journalActions = journal.Actions
            .Where(action => action.RequiredPrivilege == RequiredPrivilege.Administrator)
            .OrderBy(action => action.Sequence)
            .ToArray();
        if (journalActions.Length != receipt.Actions.Count)
        {
            throw new InvalidDataException("The administrator action identity does not match the receipt.");
        }

        var changed = journal.WasElevated != receipt.WasElevated || journal.Profile != receipt.Profile;
        journal.WasElevated = receipt.WasElevated;
        journal.Profile = receipt.Profile;
        for (var index = 0; index < journalActions.Length; index++)
        {
            var action = journalActions[index];
            var protectedAction = receipt.Actions[index];
            if (protectedAction is null
                || action.Sequence != protectedAction.Sequence
                || !string.Equals(action.ActionId, protectedAction.ActionId, StringComparison.Ordinal)
                || action.Version != protectedAction.Version
                || action.RequiredPrivilege != protectedAction.RequiredPrivilege
                || action.Reversibility != protectedAction.Reversibility)
            {
                throw new InvalidDataException("The administrator action identity does not match the receipt.");
            }

            changed |= action.State != protectedAction.State
                || action.Outcome != protectedAction.Outcome
                || action.Changed != protectedAction.Changed
                || !string.Equals(action.SnapshotJson, protectedAction.SnapshotJson, StringComparison.Ordinal)
                || action.RollbackSafeAfterInterruption != protectedAction.RollbackSafeAfterInterruption;
            action.State = protectedAction.State;
            action.Outcome = protectedAction.Outcome;
            action.Changed = protectedAction.Changed;
            action.SnapshotJson = protectedAction.SnapshotJson;
            action.RollbackSafeAfterInterruption = protectedAction.RollbackSafeAfterInterruption;
        }

        return changed;
    }

    private sealed record ReceiptPayload(
        int FormatVersion,
        Guid TransactionId,
        int SchemaVersion,
        bool WasElevated,
        OptimizationProfile? Profile,
        IReadOnlyList<ReceiptAction> Actions);

    private sealed record ReceiptAction(
        int Sequence,
        string ActionId,
        int Version,
        RequiredPrivilege RequiredPrivilege,
        ActionReversibility Reversibility,
        ActionJournalState State,
        ActionExecutionOutcome Outcome,
        bool Changed,
        string? SnapshotJson,
        bool RollbackSafeAfterInterruption);
}
