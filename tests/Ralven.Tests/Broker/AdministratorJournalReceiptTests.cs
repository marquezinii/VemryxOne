using Ralven.Broker;
using Ralven.Contracts;
using Ralven.Windows.Engine;
using Xunit;

namespace Ralven.Tests.Broker;

public sealed class AdministratorJournalReceiptTests
{
    [Fact]
    public async Task CommittedJournal_IsSealedAndAcceptedForRollback()
    {
        var journal = CreateJournal();
        var inner = new InMemoryJournalStore(journal);
        var receipts = new InMemoryReceiptStore();
        var executionStore = new BrokerAdministratorJournalStore(inner, receipts, false);

        await executionStore.SaveAsync(journal, CancellationToken.None);
        var rollbackStore = new BrokerAdministratorJournalStore(inner, receipts, true);

        Assert.Same(journal, await rollbackStore.LoadAsync(journal.TransactionId, CancellationToken.None));
    }

    [Fact]
    public async Task Rollback_RejectsLegacyJournalWithoutReceipt()
    {
        var journal = CreateJournal();
        var inner = new InMemoryJournalStore(journal);
        var receipts = new InMemoryReceiptStore();
        var rollbackStore = new BrokerAdministratorJournalStore(inner, receipts, true);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            rollbackStore.LoadAsync(journal.TransactionId, CancellationToken.None));

    }

    [Fact]
    public async Task Rollback_RestoresTamperedAdministratorStateFromReceipt()
    {
        var journal = CreateJournal();
        var inner = new InMemoryJournalStore(journal);
        var receipts = new InMemoryReceiptStore();
        receipts.Seal(journal.TransactionId, AdministratorJournalReceipt.Serialize(journal));
        var administrator = journal.Actions.Single(action =>
            action.RequiredPrivilege == RequiredPrivilege.Administrator);
        administrator.State = ActionJournalState.Applied;
        administrator.Outcome = ActionExecutionOutcome.Verified;
        administrator.Changed = false;
        administrator.SnapshotJson = "{\"value\":999}";
        var rollbackStore = new BrokerAdministratorJournalStore(inner, receipts, true);

        await rollbackStore.LoadAsync(journal.TransactionId, CancellationToken.None);

        Assert.Equal(ActionJournalState.Committed, administrator.State);
        Assert.Equal(ActionExecutionOutcome.Applied, administrator.Outcome);
        Assert.True(administrator.Changed);
        Assert.Equal("{\"value\":1}", administrator.SnapshotJson);
        Assert.Equal(1, inner.SaveCount);
    }

    [Fact]
    public async Task Rollback_RejectsDivergentAdministratorIdentity()
    {
        var journal = CreateJournal();
        var inner = new InMemoryJournalStore(journal);
        var receipts = new InMemoryReceiptStore();
        receipts.Seal(journal.TransactionId, AdministratorJournalReceipt.Serialize(journal));
        var administratorIndex = journal.Actions.FindIndex(action =>
            action.RequiredPrivilege == RequiredPrivilege.Administrator);
        journal.Actions[administratorIndex] = journal.Actions[administratorIndex] with
        {
            ActionId = "forged-action"
        };
        var rollbackStore = new BrokerAdministratorJournalStore(inner, receipts, true);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            rollbackStore.LoadAsync(journal.TransactionId, CancellationToken.None));
    }

    [Fact]
    public async Task Execution_RejectsPrecommittedAdministratorSnapshotWithoutReceipt()
    {
        var journal = CreateJournal();
        var store = new BrokerAdministratorJournalStore(
            new InMemoryJournalStore(journal),
            new InMemoryReceiptStore(),
            false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            store.LoadAsync(journal.TransactionId, CancellationToken.None));
    }

    [Fact]
    public async Task Rollback_PreservesStandardUserJournalIndependence()
    {
        var journal = CreateJournal();
        var inner = new InMemoryJournalStore(journal);
        var receipts = new InMemoryReceiptStore();
        receipts.Seal(journal.TransactionId, AdministratorJournalReceipt.Serialize(journal));
        journal.State = TransactionState.AwaitingElevationRollback;
        var standard = journal.Actions.Single(action =>
            action.RequiredPrivilege == RequiredPrivilege.StandardUser);
        standard.State = ActionJournalState.RolledBack;
        standard.Outcome = ActionExecutionOutcome.RolledBack;
        standard.SnapshotJson = "{\"user\":2}";
        var rollbackStore = new BrokerAdministratorJournalStore(inner, receipts, true);

        Assert.Same(journal, await rollbackStore.LoadAsync(journal.TransactionId, CancellationToken.None));
    }

    [Fact]
    public async Task IntermediateAdministratorSave_IsSealed()
    {
        var journal = CreateJournal();
        journal.State = TransactionState.Applying;
        var administrator = journal.Actions.Single(action =>
            action.RequiredPrivilege == RequiredPrivilege.Administrator);
        administrator.State = ActionJournalState.Applied;
        var inner = new InMemoryJournalStore(journal);
        var receipts = new InMemoryReceiptStore();
        var executionStore = new BrokerAdministratorJournalStore(inner, receipts, false);

        await executionStore.SaveAsync(journal, CancellationToken.None);

        Assert.NotNull(receipts.Read(journal.TransactionId));
        var rollbackStore = new BrokerAdministratorJournalStore(inner, receipts, true);
        Assert.Same(journal, await rollbackStore.LoadAsync(journal.TransactionId, CancellationToken.None));
    }

    [Fact]
    public async Task Rollback_ResealsTransitionsAndRestoresReplayedCommittedState()
    {
        var journal = CreateJournal();
        var inner = new InMemoryJournalStore(journal);
        var receipts = new InMemoryReceiptStore();
        var executionStore = new BrokerAdministratorJournalStore(inner, receipts, false);
        await executionStore.SaveAsync(journal, CancellationToken.None);
        var rollbackStore = new BrokerAdministratorJournalStore(inner, receipts, true);
        var administrator = journal.Actions.Single(action =>
            action.RequiredPrivilege == RequiredPrivilege.Administrator);

        await rollbackStore.LoadAsync(journal.TransactionId, CancellationToken.None);
        journal.State = TransactionState.RollingBack;
        administrator.State = ActionJournalState.RollingBack;
        await rollbackStore.SaveAsync(journal, CancellationToken.None);
        Assert.Same(journal, await rollbackStore.LoadAsync(journal.TransactionId, CancellationToken.None));

        journal.State = TransactionState.RolledBack;
        administrator.State = ActionJournalState.RolledBack;
        administrator.Outcome = ActionExecutionOutcome.RolledBack;
        await rollbackStore.SaveAsync(journal, CancellationToken.None);
        Assert.Same(journal, await rollbackStore.LoadAsync(journal.TransactionId, CancellationToken.None));

        journal.State = TransactionState.Committed;
        administrator.State = ActionJournalState.Committed;
        administrator.Outcome = ActionExecutionOutcome.Applied;
        await rollbackStore.LoadAsync(journal.TransactionId, CancellationToken.None);
        Assert.Equal(ActionJournalState.RolledBack, administrator.State);
        Assert.Equal(ActionExecutionOutcome.RolledBack, administrator.Outcome);
    }

    [Fact]
    public async Task ReceiptRecoversWhenJournalSaveFailsAfterSeal()
    {
        var staleJournal = CreateJournal();
        var latestJournal = Clone(staleJournal);
        var administrator = latestJournal.Actions.Single(action =>
            action.RequiredPrivilege == RequiredPrivilege.Administrator);
        administrator.State = ActionJournalState.Applied;
        administrator.Outcome = ActionExecutionOutcome.Applied;
        administrator.SnapshotJson = "{\"trusted\":true}";
        var inner = new FailFirstSaveJournalStore(staleJournal);
        var receipts = new InMemoryReceiptStore();
        var executionStore = new BrokerAdministratorJournalStore(inner, receipts, false);

        await Assert.ThrowsAsync<IOException>(() =>
            executionStore.SaveAsync(latestJournal, CancellationToken.None));
        Assert.NotNull(receipts.Read(latestJournal.TransactionId));

        var recoveryStore = new BrokerAdministratorJournalStore(inner, receipts, true);
        var recovered = await recoveryStore.LoadAsync(latestJournal.TransactionId, CancellationToken.None);
        var recoveredAdministrator = recovered!.Actions.Single(action =>
            action.RequiredPrivilege == RequiredPrivilege.Administrator);
        Assert.Equal(ActionJournalState.Applied, recoveredAdministrator.State);
        Assert.Equal("{\"trusted\":true}", recoveredAdministrator.SnapshotJson);
        Assert.Equal(2, inner.SaveAttempts);
    }

    [Fact]
    public async Task Commit_FailsClosedWhenReceiptCannotBeSealed()
    {
        var journal = CreateJournal();
        var inner = new InMemoryJournalStore(journal);
        var store = new BrokerAdministratorJournalStore(
            inner,
            new ThrowingReceiptStore(),
            false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            store.SaveAsync(journal, CancellationToken.None));
        Assert.Equal(0, inner.SaveCount);
    }

    [Fact]
    public async Task Rollback_RejectsCorruptAndOversizedProtectedPayloads()
    {
        var journal = CreateJournal();
        var inner = new InMemoryJournalStore(journal);
        var receipts = new InMemoryReceiptStore();
        var store = new BrokerAdministratorJournalStore(inner, receipts, true);

        receipts.Seal(journal.TransactionId, "not-json"u8.ToArray());
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            store.LoadAsync(journal.TransactionId, CancellationToken.None));

        receipts.Seal(
            journal.TransactionId,
            new byte[RegistryAdministratorJournalReceiptStore.MaximumPayloadBytes + 1]);
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            store.LoadAsync(journal.TransactionId, CancellationToken.None));
    }

    private static WindowsTransactionJournal Clone(WindowsTransactionJournal journal) => journal with
    {
        Actions = journal.Actions.Select(action => action with
        {
            Messages = [.. action.Messages]
        }).ToList()
    };

    private static WindowsTransactionJournal CreateJournal()
    {
        var transactionId = Guid.NewGuid();
        return new WindowsTransactionJournal
        {
            TransactionId = transactionId,
            SchemaVersion = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            WasElevated = true,
            Profile = OptimizationProfile.Balanced,
            State = TransactionState.Committed,
            Actions =
            [
                new WindowsActionJournalEntry
                {
                    Sequence = 1,
                    ActionId = "standard-user-action",
                    Version = 1,
                    RequiredPrivilege = RequiredPrivilege.StandardUser,
                    Reversibility = ActionReversibility.FullyReversible,
                    State = ActionJournalState.Committed,
                    Outcome = ActionExecutionOutcome.Applied,
                    Changed = true,
                    SnapshotJson = "{\"user\":1}"
                },
                new WindowsActionJournalEntry
                {
                    Sequence = 2,
                    ActionId = "administrator-action",
                    Version = 1,
                    RequiredPrivilege = RequiredPrivilege.Administrator,
                    Reversibility = ActionReversibility.FullyReversible,
                    State = ActionJournalState.Committed,
                    Outcome = ActionExecutionOutcome.Applied,
                    Changed = true,
                    SnapshotJson = "{\"value\":1}"
                }
            ]
        };
    }

    private sealed class InMemoryJournalStore(WindowsTransactionJournal journal) : IWindowsTransactionJournalStore
    {
        public int SaveCount { get; private set; }

        public Task SaveAsync(
            WindowsTransactionJournal value,
            CancellationToken cancellationToken)
        {
            journal = value;
            SaveCount++;
            return Task.CompletedTask;
        }

        public Task<WindowsTransactionJournal?> LoadAsync(
            Guid transactionId,
            CancellationToken cancellationToken) => Task.FromResult<WindowsTransactionJournal?>(journal);
    }

    private sealed class FailFirstSaveJournalStore(WindowsTransactionJournal journal)
        : IWindowsTransactionJournalStore
    {
        public int SaveAttempts { get; private set; }

        public Task SaveAsync(WindowsTransactionJournal value, CancellationToken cancellationToken)
        {
            SaveAttempts++;
            if (SaveAttempts == 1)
            {
                throw new IOException("Simulated journal persistence failure.");
            }

            journal = value;
            return Task.CompletedTask;
        }

        public Task<WindowsTransactionJournal?> LoadAsync(
            Guid transactionId,
            CancellationToken cancellationToken) => Task.FromResult<WindowsTransactionJournal?>(journal);
    }

    private sealed class InMemoryReceiptStore : IAdministratorJournalReceiptStore
    {
        private readonly Dictionary<Guid, byte[]> receipts = [];

        public void Seal(Guid transactionId, byte[] payload) => receipts[transactionId] = payload;

        public byte[]? Read(Guid transactionId) => receipts.GetValueOrDefault(transactionId);
    }

    private sealed class ThrowingReceiptStore : IAdministratorJournalReceiptStore
    {
        public void Seal(Guid transactionId, byte[] payload) =>
            throw new UnauthorizedAccessException("Simulated HKLM write failure.");

        public byte[]? Read(Guid transactionId) => null;
    }
}
