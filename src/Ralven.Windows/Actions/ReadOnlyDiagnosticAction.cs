namespace Ralven.Windows.Actions;

/// <summary>Resolves a user-facing action result at the application boundary.</summary>
public delegate string WindowsActionTextResolver(string key, params object?[] arguments);

/// <summary>
/// Base for the read-only diagnostics. They never change the machine: applying
/// one always resolves to <see cref="WindowsActionApplyResult.NoChange"/> with
/// a single informative message, there is no snapshot to keep and rollback is a
/// no-op. Derived actions only produce the message and must degrade a signal
/// they could not read into an honest "not available" text -- a diagnostic that
/// throws would abort a run it was never allowed to change.
/// </summary>
public abstract class ReadOnlyDiagnosticAction : WindowsOptimizationAction
{
    public sealed override Task<WindowsActionApplyResult> ApplyAsync(
        WindowsActionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(WindowsActionApplyResult.NoChange(Describe()));
    }

    public sealed override Task RollbackAsync(
        WindowsActionContext context,
        string? snapshotJson,
        CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>Builds the message reported to the user for this diagnostic.</summary>
    protected abstract string Describe();
}

/// <summary>
/// Read-only diagnostic counterpart for inspectors that already expose an
/// asynchronous contract. Keeping this separate prevents synchronous actions
/// from allocating an async state machine and avoids blocking on async I/O.
/// </summary>
public abstract class AsyncReadOnlyDiagnosticAction : WindowsOptimizationAction
{
    public sealed override async Task<WindowsActionApplyResult> ApplyAsync(
        WindowsActionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var message = await DescribeAsync(cancellationToken).ConfigureAwait(false);
        return WindowsActionApplyResult.NoChange(message);
    }

    public sealed override Task RollbackAsync(
        WindowsActionContext context,
        string? snapshotJson,
        CancellationToken cancellationToken) => Task.CompletedTask;

    protected abstract Task<string> DescribeAsync(CancellationToken cancellationToken);
}
