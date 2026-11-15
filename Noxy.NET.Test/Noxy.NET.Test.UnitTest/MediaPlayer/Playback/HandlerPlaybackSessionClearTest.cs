using Noxy.NET.MediaPlayer.Application.Handlers.Playback;
using Noxy.NET.MediaPlayer.Application.Notifications.Playback;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playback;

public class HandlerPlaybackSessionClearTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly HandlerPlaybackSessionClear _handler;

    public HandlerPlaybackSessionClearTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), _publisher);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private void Seed(Guid userID)
    {
        _store.PlaybackSessionList.Add(new() { UserID = userID, QueueMediaFileIDList = [Guid.CreateVersion7()], CurrentIndex = 0 });
    }

    [Fact]
    public async Task Clear_RemovesTheSession_CommitsOnce_AndPublishesAfterTheCommitWithTheToken()
    {
        Seed(UserID);
        List<int> commitsAtPublishList = [];
        _publisher.OnPublish = _ => commitsAtPublishList.Add(_store.CommitCount);
        int commits = _store.CommitCount;
        using CancellationTokenSource source = new();

        await _handler.Handle(new(UserID), source.Token);

        Assert.Empty(_store.PlaybackSessionList);
        Assert.Equal(commits + 1, _store.CommitCount);
        Assert.Equal([commits + 1], commitsAtPublishList);
        Assert.Equal(UserID, Assert.Single(_publisher.GetList<NotificationPlaybackSessionCleared>()).UserID);
        Assert.Equal(source.Token, Assert.Single(_publisher.TokenList));
    }

    [Fact]
    public async Task Clear_WithNoSession_DoesNothing_AndPublishesNothing()
    {
        int commits = _store.CommitCount;

        await _handler.Handle(new(UserID), Token);

        Assert.Equal(commits, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Clear_LeavesAnotherUsersSessionAlone()
    {
        Guid other = Guid.CreateVersion7();
        Seed(UserID);
        Seed(other);

        await _handler.Handle(new(UserID), Token);

        Assert.Equal(other, Assert.Single(_store.PlaybackSessionList).UserID);
    }

    [Fact]
    public async Task Clear_WhenTheCommitFails_PublishesNothing()
    {
        Seed(UserID);
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new(UserID), Token).AsTask());

        Assert.Empty(_publisher.PublishedList);
    }
}
