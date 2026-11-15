using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Playback;

public class EntityPlaybackSessionExtensionTest
{
    private static readonly Guid A = Guid.CreateVersion7();
    private static readonly Guid B = Guid.CreateVersion7();
    private static readonly Guid C = Guid.CreateVersion7();

    private static EntityPlaybackSession Session(List<Guid> queue, int? index, double position = 30)
    {
        return new() { UserID = Guid.CreateVersion7(), QueueMediaFileIDList = queue, CurrentIndex = index, PositionSeconds = position };
    }

    [Fact]
    public void SetModified_IsAlwaysLaterThanTheTimeBefore_EvenWhenTheClockHasNotMoved()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        EntityPlaybackSession session = Session([A], 0);
        session.TimeModified = start;

        session.SetModified(start);
        Assert.True(session.TimeModified > start);

        session.SetModified(start.AddSeconds(5));
        Assert.Equal(start.AddSeconds(5), session.TimeModified);

        session.SetModified(start);
        Assert.True(session.TimeModified > start.AddSeconds(5));
    }

    [Fact]
    public void RemoveMediaFile_NotInTheQueue_ChangesNothing()
    {
        EntityPlaybackSession session = Session([A, B], 1);

        bool changed = session.RemoveMediaFile(C);

        Assert.False(changed);
        Assert.Equal([A, B], session.QueueMediaFileIDList);
        Assert.Equal(1, session.CurrentIndex);
        Assert.Equal(30, session.PositionSeconds);
    }

    [Fact]
    public void RemoveMediaFile_RemovesEveryOccurrenceAndKeepsTheOrderOfTheRest()
    {
        EntityPlaybackSession session = Session([A, B, A, C, A], null);

        bool changed = session.RemoveMediaFile(A);

        Assert.True(changed);
        Assert.Equal([B, C], session.QueueMediaFileIDList);
    }

    [Fact]
    public void RemoveMediaFile_BeforeTheCurrentOne_MovesTheIndexBackByHowManyWereRemovedBeforeIt()
    {
        EntityPlaybackSession session = Session([A, B, A, C], 3);

        session.RemoveMediaFile(A);

        Assert.Equal([B, C], session.QueueMediaFileIDList);
        Assert.Equal(1, session.CurrentIndex);
        Assert.Equal(30, session.PositionSeconds);
    }

    [Fact]
    public void RemoveMediaFile_AfterTheCurrentOne_KeepsTheIndexAndThePosition()
    {
        EntityPlaybackSession session = Session([A, B, C], 0);

        session.RemoveMediaFile(C);

        Assert.Equal([A, B], session.QueueMediaFileIDList);
        Assert.Equal(0, session.CurrentIndex);
        Assert.Equal(30, session.PositionSeconds);
    }

    [Fact]
    public void RemoveMediaFile_TheCurrentOneItself_ClearsTheIndexAndStartsThePositionOver()
    {
        EntityPlaybackSession session = Session([A, B, A], 2);

        session.RemoveMediaFile(A);

        Assert.Equal([B], session.QueueMediaFileIDList);
        Assert.Null(session.CurrentIndex);
        Assert.Equal(0, session.PositionSeconds);
    }

    [Fact]
    public void RemoveMediaFile_WithNothingPlaying_KeepsTheIndexNullAndThePosition()
    {
        EntityPlaybackSession session = Session([A, B], null);

        session.RemoveMediaFile(A);

        Assert.Null(session.CurrentIndex);
        Assert.Equal(30, session.PositionSeconds);
    }

    [Fact]
    public void RemoveMediaFile_TheOnlyOne_LeavesAnEmptyQueueWithNoIndex()
    {
        EntityPlaybackSession session = Session([A, A], 1);

        session.RemoveMediaFile(A);

        Assert.Empty(session.QueueMediaFileIDList);
        Assert.Null(session.CurrentIndex);
    }
}
