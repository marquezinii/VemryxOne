using Ralven.Windows.Infrastructure;

namespace Ralven.App.Services;

internal sealed class FiveMSessionStateTracker
{
    private int consecutiveAbsences;
    private DateTimeOffset? firstAbsenceAt;

    public bool IsActive { get; private set; }

    public bool IsEndConfirmationPending => IsActive && consecutiveAbsences == 1;

    public bool HasCompletedSession { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public TimeSpan? LastDuration { get; private set; }

    public void Reset()
    {
        consecutiveAbsences = 0;
        firstAbsenceAt = null;
        IsActive = false;
        HasCompletedSession = false;
        StartedAt = null;
        LastDuration = null;
    }

    public void Observe(FiveMSessionPresence presence, DateTimeOffset observedAt)
    {
        if (presence == FiveMSessionPresence.Present)
        {
            consecutiveAbsences = 0;
            firstAbsenceAt = null;
            if (!IsActive)
            {
                IsActive = true;
                HasCompletedSession = false;
                StartedAt = observedAt;
                LastDuration = null;
            }

            return;
        }

        if (!IsActive)
        {
            return;
        }

        if (presence == FiveMSessionPresence.Indeterminate)
        {
            // Inconclusive reading: it neither confirms presence nor should it
            // erase an absence streak already in progress, or an intermittent
            // read failure could keep the session "active" forever.
            return;
        }

        consecutiveAbsences++;
        if (consecutiveAbsences < 2)
        {
            firstAbsenceAt = observedAt;
            return;
        }

        var startedAt = StartedAt ?? observedAt;
        var endedAt = firstAbsenceAt ?? observedAt;
        LastDuration = endedAt > startedAt ? endedAt - startedAt : TimeSpan.Zero;
        consecutiveAbsences = 0;
        firstAbsenceAt = null;
        IsActive = false;
        HasCompletedSession = true;
        StartedAt = null;
    }
}
