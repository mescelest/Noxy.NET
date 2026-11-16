using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Commands.Playback;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Handlers.Playback;
using Noxy.NET.MediaPlayer.Application.Notifications.Playback;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playback;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playback;

public class HandlerPlaybackSessionSaveTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerPlaybackSessionSave _handler;

    public HandlerPlaybackSessionSaveTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), Options.Create(new ConfigPlayback { MaxQueueLength = 5 }), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private void Advance(int seconds = 1)
    {
        _time.Now = _time.Now.AddSeconds(seconds);
    }

    private Guid MediaFile(Guid? userID = null)
    {
        EntityMediaFile file = new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 3,
            ContentHash = new string('a', 64),
            UserID = userID ?? UserID
        };

        _store.MediaFileList.Add(file);
        return file.ID;
    }

    private List<Guid> MediaFileList(int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => MediaFile())];
    }

    private Guid Playlist(Guid? userID = null)
    {
        EntityPlaylist playlist = new() { UserID = userID ?? UserID, Name = "Mix" };
        _store.PlaylistList.Add(playlist);
        return playlist.ID;
    }

    private async Task<ResponsePlaybackSession> Save(List<Guid> queue, int? index = null, double position = 0, Guid? playlistID = null, CancellationToken? token = null)
    {
        return await _handler.Handle(new CommandPlaybackSessionSave(UserID, queue, index, position, playlistID), token ?? Token);
    }

    private EntityPlaybackSession Stored()
    {
        return _store.PlaybackSessionList.Single(x => x.UserID == UserID);
    }

    [Fact]
    public async Task Save_TheFirstTime_CreatesTheSessionWithAllItsFields()
    {
        List<Guid> queue = MediaFileList(3);
        Guid playlistID = Playlist();

        ResponsePlaybackSession response = await Save(queue, 1, 12.5, playlistID);

        Assert.Equal(queue, response.QueueMediaFileIDList);
        Assert.Equal(1, response.CurrentIndex);
        Assert.Equal(12.5, response.PositionSeconds);
        Assert.Equal(playlistID, response.PlaylistID);
        Assert.Equal(_time.Now.UtcDateTime, response.TimeModified);
        Assert.Equal(response.ID, Stored().ID);
        Assert.Equal(queue, Stored().QueueMediaFileIDList);
    }

    [Fact]
    public async Task Save_CommitsOnce_ThenPublishesTheSessionWithTheTokenAfterTheCommit()
    {
        List<Guid> queue = MediaFileList(2);
        List<int> commitsAtPublishList = [];
        _publisher.OnPublish = _ => commitsAtPublishList.Add(_store.CommitCount);
        int commits = _store.CommitCount;
        using CancellationTokenSource source = new();

        ResponsePlaybackSession response = await Save(queue, 0, 1, token: source.Token);

        Assert.Equal(commits + 1, _store.CommitCount);
        Assert.Equal([commits + 1], commitsAtPublishList);
        NotificationPlaybackSessionUpdated published = Assert.Single(_publisher.GetList<NotificationPlaybackSessionUpdated>());
        Assert.Equal(UserID, published.UserID);
        Assert.Equal(response.ID, published.Session.ID);
        Assert.Equal(source.Token, Assert.Single(_publisher.TokenList));
    }

    [Fact]
    public async Task Save_AgainReplacesTheSession_KeepsItsIDAndSetsANewModifiedTime()
    {
        ResponsePlaybackSession first = await Save(MediaFileList(2), 0, 5);
        Advance();
        List<Guid> second = MediaFileList(3);

        ResponsePlaybackSession response = await Save(second, 2, 40);

        Assert.Equal(first.ID, response.ID);
        Assert.Equal(second, response.QueueMediaFileIDList);
        Assert.Equal(2, response.CurrentIndex);
        Assert.Equal(40, response.PositionSeconds);
        Assert.True(response.TimeModified > first.TimeModified);
        Assert.Single(_store.PlaybackSessionList);
        Assert.Equal(2, _publisher.GetList<NotificationPlaybackSessionUpdated>().Count);
    }

    [Fact]
    public async Task Save_TheSameStateAgain_NeitherCommitsNorPublishes_AndKeepsTheModifiedTime()
    {
        List<Guid> queue = MediaFileList(2);
        Guid playlistID = Playlist();
        ResponsePlaybackSession first = await Save(queue, 1, 7.25, playlistID);
        int commits = _store.CommitCount;
        Advance(30);

        ResponsePlaybackSession again = await Save([.. queue], 1, 7.25, playlistID);

        Assert.Equal(commits, _store.CommitCount);
        Assert.Single(_publisher.GetList<NotificationPlaybackSessionUpdated>());
        Assert.Equal(first.TimeModified, again.TimeModified);
        Assert.Equal(first.ID, again.ID);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(-0.1)]
    [InlineData(HandlerPlaybackSessionSave.PositionToleranceSeconds)]
    [InlineData(-HandlerPlaybackSessionSave.PositionToleranceSeconds)]
    public async Task Save_APositionWithinTheToleranceOfTheStoredOne_IsTheSameState_AndSavesAndPushesNothing(double difference)
    {
        List<Guid> queue = MediaFileList(1);
        ResponsePlaybackSession first = await Save(queue, 0, 20);
        int commits = _store.CommitCount;

        ResponsePlaybackSession again = await Save(queue, 0, 20 + difference);

        Assert.Equal(commits, _store.CommitCount);
        Assert.Single(_publisher.GetList<NotificationPlaybackSessionUpdated>());
        Assert.Equal(first.PositionSeconds, again.PositionSeconds);
        Assert.Equal(first.TimeModified, again.TimeModified);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(HandlerPlaybackSessionSave.PositionToleranceSeconds + 0.01)]
    public async Task Save_APositionBeyondTheTolerance_IsAChange(double difference)
    {
        List<Guid> queue = MediaFileList(1);
        await Save(queue, 0, 20);
        Advance();

        ResponsePlaybackSession changed = await Save(queue, 0, 20 + difference);

        Assert.Equal(20 + difference, changed.PositionSeconds);
        Assert.Equal(2, _publisher.GetList<NotificationPlaybackSessionUpdated>().Count);
    }

    [Fact]
    public async Task Save_AChangeAtTheSameClockTime_StillGetsALaterModifiedTime_SoTheETagMoves()
    {
        List<Guid> queue = MediaFileList(2);
        ResponsePlaybackSession first = await Save(queue, 0, 1);

        ResponsePlaybackSession second = await Save(queue, 0, 2);

        Assert.True(second.TimeModified > first.TimeModified);
    }

    [Fact]
    public async Task Save_AQueueMayHoldTheSameMediaFileMoreThanOnce()
    {
        Guid media = MediaFile();

        ResponsePlaybackSession response = await Save([media, media, media], 2);

        Assert.Equal([media, media, media], response.QueueMediaFileIDList);
    }

    [Fact]
    public async Task Save_AnEmptyQueueWithNoCurrentIndex_IsAllowed()
    {
        ResponsePlaybackSession response = await Save([]);

        Assert.Empty(response.QueueMediaFileIDList);
        Assert.Null(response.CurrentIndex);
    }

    [Fact]
    public async Task Save_AMediaFileOfAnotherUserOrAnUnknownOne_IsNotFound_AndNothingIsSavedOrPublished()
    {
        Guid mine = MediaFile();
        Guid foreign = MediaFile(Guid.CreateVersion7());

        await Assert.ThrowsAsync<MediaNotFoundException>(() => Save([mine, foreign]));
        await Assert.ThrowsAsync<MediaNotFoundException>(() => Save([mine, Guid.CreateVersion7()]));

        Assert.Empty(_store.PlaybackSessionList);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Save_AQueueOfExactlyTheMaximum_IsAllowed_AndOneMoreIsInvalid()
    {
        await Save(MediaFileList(5), 4);

        await Assert.ThrowsAsync<InvalidPlaybackSessionException>(() => Save(MediaFileList(6), 0));

        Assert.Equal(5, Stored().QueueMediaFileIDList.Count);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(100)]
    public async Task Save_AnIndexOutsideTheQueue_IsInvalid(int index)
    {
        List<Guid> queue = MediaFileList(3);

        await Assert.ThrowsAsync<InvalidPlaybackSessionException>(() => Save(queue, index));

        Assert.Empty(_store.PlaybackSessionList);
    }

    [Fact]
    public async Task Save_AnyIndexForAnEmptyQueue_IsInvalid()
    {
        await Assert.ThrowsAsync<InvalidPlaybackSessionException>(() => Save([], 0));
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public async Task Save_ANegativeOrNonFinitePosition_IsInvalid(double position)
    {
        List<Guid> queue = MediaFileList(1);

        await Assert.ThrowsAsync<InvalidPlaybackSessionException>(() => Save(queue, 0, position));

        Assert.Empty(_store.PlaybackSessionList);
    }

    [Fact]
    public async Task Save_APlaylistOfAnotherUserOrAnUnknownOne_IsNotFound_AndOwnOrNoneIsAllowed()
    {
        List<Guid> queue = MediaFileList(1);

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => Save(queue, 0, 0, Playlist(Guid.CreateVersion7())));
        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => Save(queue, 0, 0, Guid.CreateVersion7()));
        Assert.Empty(_store.PlaybackSessionList);

        Guid own = Playlist();
        Assert.Equal(own, (await Save(queue, 0, 0, own)).PlaylistID);
        Assert.Null((await Save(queue, 0, 1)).PlaylistID);
    }

    [Fact]
    public async Task Save_WhenTheCommitFails_PublishesNothing()
    {
        List<Guid> queue = MediaFileList(1);
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Save(queue, 0));

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Save_WhenTwoDevicesCreateTheFirstSessionAtOnce_TheLoserRunsAgainAsAnUpdate_AndTheLastWriteWins()
    {
        List<Guid> queue = MediaFileList(2);
        _store.KeyConflictCommitCount = 1;

        ResponsePlaybackSession response = await Save(queue, 1, 9);

        Assert.Equal(9, Stored().PositionSeconds);
        Assert.Equal(response.ID, Stored().ID);
        Assert.Single(_publisher.GetList<NotificationPlaybackSessionUpdated>());
    }

    [Fact]
    public async Task Save_AnotherUsersSession_IsNeverTouched()
    {
        Guid other = Guid.CreateVersion7();
        _store.PlaybackSessionList.Add(new() { UserID = other, QueueMediaFileIDList = [MediaFile(other)], PositionSeconds = 3 });

        await Save(MediaFileList(1), 0, 8);

        EntityPlaybackSession untouched = _store.PlaybackSessionList.Single(x => x.UserID == other);
        Assert.Equal(3, untouched.PositionSeconds);
        Assert.Equal(2, _store.PlaybackSessionList.Count);
    }
}
