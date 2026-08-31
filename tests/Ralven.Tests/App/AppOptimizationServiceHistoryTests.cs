using System.Globalization;
using System.Text.Json;
using Ralven.App.Services;
using Ralven.Contracts;
using Ralven.Core.Catalog;
using Ralven.Windows.Engine;
using Xunit;

namespace Ralven.Tests.App;

public sealed class AppOptimizationServiceHistoryTests
{
    [Fact]
    public async Task LoadHistory_UsesPersistedProfileInsteadOfActionNameHeuristics()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var transactionId = Guid.NewGuid();
        var definition = ActionCatalog.Current.GetRequired(
            OptimizationActionIds.ReduceWindowsVisualEffects);
        var journal = new WindowsTransactionJournal
        {
            TransactionId = transactionId,
            SchemaVersion = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            WasElevated = false,
            Profile = OptimizationProfile.Aggressive,
            State = TransactionState.Committed,
            Actions =
            [
                new WindowsActionJournalEntry
                {
                    Sequence = 1,
                    ActionId = definition.Id,
                    Version = definition.Version,
                    RequiredPrivilege = definition.RequiredPrivilege,
                    Reversibility = definition.Reversibility,
                    State = ActionJournalState.Committed,
                    Outcome = ActionExecutionOutcome.Applied,
                    Changed = true,
                    SnapshotJson = "{}"
                }
            ]
        };
        await WriteJournalAsync(temporaryDirectory.Path, journal);

        var history = await new AppOptimizationService(temporaryDirectory.Path)
            .LoadHistoryAsync(TestContext.Current.CancellationToken);

        Assert.Equal(OptimizationProfile.Aggressive, Assert.Single(history).Profile);
    }

    [Fact]
    public async Task LoadHistory_LegacyJournalInfersAggressiveVisualEffectsProfile()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var definition = ActionCatalog.Current.GetRequired(
            OptimizationActionIds.ReduceWindowsVisualEffects);
        var journal = Journal(definition, TransactionState.Committed) with
        {
            Profile = null
        };
        await WriteJournalAsync(temporaryDirectory.Path, journal);

        var history = await new AppOptimizationService(temporaryDirectory.Path)
            .LoadHistoryAsync(TestContext.Current.CancellationToken);

        Assert.Equal(OptimizationProfile.Aggressive, Assert.Single(history).Profile);
    }

    [Theory]
    [InlineData(OptimizationActionIds.ApplyGtaVGraphicsLaunchParameters)]
    [InlineData(OptimizationActionIds.RepairStaleAuthData)]
    public async Task LoadHistory_LegacySnapshotVersionDoesNotOfferUnsafeRollback(string actionId)
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var definition = ActionCatalog.Current.GetRequired(actionId);
        Assert.Equal(2, definition.Version);
        var journal = Journal(definition, TransactionState.CommittedWithErrors);
        journal.Actions[0] = journal.Actions[0] with
        {
            Version = 1,
            State = ActionJournalState.Failed,
            Outcome = ActionExecutionOutcome.Failed,
            RollbackSafeAfterInterruption = true
        };
        await WriteJournalAsync(temporaryDirectory.Path, journal);

        var history = await new AppOptimizationService(temporaryDirectory.Path)
            .LoadHistoryAsync(TestContext.Current.CancellationToken);

        Assert.False(Assert.Single(history).CanRollback);
    }

    [Fact]
    public async Task LoadHistory_InterruptedRebuildableApplyOffersSafeRollback()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var definition = ActionCatalog.Current.GetRequired(
            OptimizationActionIds.RepairLegacyServerCache);
        var journal = Journal(definition, TransactionState.CommittedWithErrors);
        journal.Actions[0] = journal.Actions[0] with
        {
            State = ActionJournalState.Failed,
            Outcome = ActionExecutionOutcome.Failed,
            RollbackSafeAfterInterruption = true
        };
        await WriteJournalAsync(temporaryDirectory.Path, journal);

        var history = await new AppOptimizationService(temporaryDirectory.Path)
            .LoadHistoryAsync(TestContext.Current.CancellationToken);

        Assert.True(Assert.Single(history).CanRollback);
    }

    [Fact]
    public async Task LoadHistory_LabelsCommittedWithErrorsHonestly()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var definition = ActionCatalog.Current.GetRequired(OptimizationActionIds.EnableGameMode);
        await WriteJournalAsync(
            temporaryDirectory.Path,
            Journal(definition, TransactionState.CommittedWithErrors));
        var localization = new LocalizationService(CultureInfo.GetCultureInfo("pt-BR"));

        var history = await new AppOptimizationService(temporaryDirectory.Path, localization)
            .LoadHistoryAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            localization.GetString("History.State.CommittedWithErrors"),
            Assert.Single(history).State);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("pt-BR")]
    [InlineData("es")]
    public void AdministrativeFailureMessages_AreGenericForAnyBrokerAction(string cultureName)
    {
        var localization = new LocalizationService(CultureInfo.GetCultureInfo(cultureName));

        var cancelled = localization.GetString("Runtime.UacCancelledPreserved");
        var failed = localization.Format("Runtime.AdminPhaseFailedPreserved", "failure");

        Assert.DoesNotContain("power plan", cancelled, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("plano de energia", cancelled, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("plan de energía", cancelled, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("power plan", failed, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("plano de energia", failed, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("plan de energía", failed, StringComparison.OrdinalIgnoreCase);
    }

    private static WindowsTransactionJournal Journal(
        OptimizationActionDefinition definition,
        TransactionState state)
    {
        return new WindowsTransactionJournal
        {
            TransactionId = Guid.NewGuid(),
            SchemaVersion = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            WasElevated = false,
            Profile = OptimizationProfile.Balanced,
            State = state,
            Actions =
            [
                new WindowsActionJournalEntry
                {
                    Sequence = 1,
                    ActionId = definition.Id,
                    Version = definition.Version,
                    RequiredPrivilege = definition.RequiredPrivilege,
                    Reversibility = definition.Reversibility,
                    State = ActionJournalState.Committed,
                    Outcome = ActionExecutionOutcome.Applied,
                    Changed = true,
                    SnapshotJson = "{}"
                }
            ]
        };
    }

    private static async Task WriteJournalAsync(
        string appDataDirectory,
        WindowsTransactionJournal journal)
    {
        var journalDirectory = Path.Combine(appDataDirectory, "Transactions");
        Directory.CreateDirectory(journalDirectory);
        await File.WriteAllTextAsync(
            Path.Combine(journalDirectory, $"{journal.TransactionId:N}.json"),
            JsonSerializer.Serialize(journal, RalvenJson.Options),
            TestContext.Current.CancellationToken);
    }
}
