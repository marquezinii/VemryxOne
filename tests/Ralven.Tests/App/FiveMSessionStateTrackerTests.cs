using Ralven.App.Services;
using Ralven.Windows.Infrastructure;
using Xunit;

namespace Ralven.Tests.App;

public sealed class FiveMSessionStateTrackerTests
{
    private static readonly DateTimeOffset Start = new(2026, 8, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Observe_RequiresTwoConsecutiveAbsencesToCompleteSession()
    {
        var tracker = new FiveMSessionStateTracker();

        tracker.Observe(FiveMSessionPresence.Present, Start);
        tracker.Observe(FiveMSessionPresence.AbsentConfirmed, Start.AddSeconds(5));

        Assert.True(tracker.IsActive);
        Assert.True(tracker.IsEndConfirmationPending);
        Assert.False(tracker.HasCompletedSession);

        tracker.Observe(FiveMSessionPresence.AbsentConfirmed, Start.AddSeconds(10));

        Assert.False(tracker.IsActive);
        Assert.False(tracker.IsEndConfirmationPending);
        Assert.True(tracker.HasCompletedSession);
        Assert.Equal(TimeSpan.FromSeconds(5), tracker.LastDuration);
    }

    [Theory]
    [InlineData(FiveMSessionPresence.Present)]
    [InlineData(FiveMSessionPresence.Indeterminate)]
    public void Observe_NonAbsentReadingBreaksEndConfirmation(FiveMSessionPresence reading)
    {
        var tracker = new FiveMSessionStateTracker();
        tracker.Observe(FiveMSessionPresence.Present, Start);
        tracker.Observe(FiveMSessionPresence.AbsentConfirmed, Start.AddSeconds(5));

        tracker.Observe(reading, Start.AddSeconds(10));
        tracker.Observe(FiveMSessionPresence.AbsentConfirmed, Start.AddSeconds(15));

        Assert.True(tracker.IsActive);
        Assert.True(tracker.IsEndConfirmationPending);
        Assert.False(tracker.HasCompletedSession);
    }

    [Fact]
    public void Observe_PresentAfterCompletionStartsANewSession()
    {
        var tracker = new FiveMSessionStateTracker();
        tracker.Observe(FiveMSessionPresence.Present, Start);
        tracker.Observe(FiveMSessionPresence.AbsentConfirmed, Start.AddSeconds(5));
        tracker.Observe(FiveMSessionPresence.AbsentConfirmed, Start.AddSeconds(10));

        var nextStart = Start.AddMinutes(1);
        tracker.Observe(FiveMSessionPresence.Present, nextStart);

        Assert.True(tracker.IsActive);
        Assert.False(tracker.HasCompletedSession);
        Assert.Equal(nextStart, tracker.StartedAt);
        Assert.Null(tracker.LastDuration);
    }
}
