using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Handlers.Playlist;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class HandlerPlaylistPlayedTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerPlaylistPlayed _handler;

    public HandlerPlaylistPlayedTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), Options.Create(new ConfigPlayback { PlayedWindowSeconds = 10 }), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private void Advance(int seconds)
    {
        _time.Now = _time.Now.AddSeconds(seconds);
    }

    private EntityPlaylist Seed(Guid? userID = null, int entryCount = 0)
    {
        DateTime now = _time.Now.UtcDateTime;
        EntityPlaylist playlist = new()
        {
            UserID = userID ?? UserID,
            Name = "Mix",
            TimeCreated = now,
            TimeModified = now
        };

        for (int index = 0; index < entryCount; index++)
        {
            playlist.EntryList.Add(new() { PlaylistID = playlist.ID, MediaFileID = Guid.CreateVersion7(), Position = index });
        }

        _store.PlaylistList.Add(playlist);
        return playlist;
    }

    private EntityPlaylist Stored(Guid id)
    {
        return _store.PlaylistList.Single(x => x.ID == id);
    }

    [Fact]
    public async Task Handle_SetsTheLastPlayedTime_ButNeverTheModifiedTime()
    {
        EntityPlaylist playlist = Seed();
        DateTime modified = playlist.TimeModified;
        Advance(60);

        await _handler.Handle(new(UserID, playlist.ID), Token);

        Assert.Equal(_time.Now.UtcDateTime, Stored(playlist.ID).TimeLastPlayed);
        Assert.Equal(modified, Stored(playlist.ID).TimeModified);
    }

    [Fact]
    public async Task Handle_CommitsOnce_ThenPublishesTheSummaryWithTheStampAndTheTokenAfterTheCommit()
    {
        EntityPlaylist playlist = Seed(entryCount: 3);
        List<int> commitsAtPublishList = [];
        _publisher.OnPublish = _ => commitsAtPublishList.Add(_store.CommitCount);
        int commits = _store.CommitCount;
        using CancellationTokenSource source = new();

        await _handler.Handle(new(UserID, playlist.ID), source.Token);

        Assert.Equal(commits + 1, _store.CommitCount);
        Assert.Equal([commits + 1], commitsAtPublishList);
        NotificationPlaylistUpdated published = Assert.Single(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Equal(UserID, published.UserID);
        Assert.Equal(playlist.ID, published.Playlist.ID);
        Assert.Equal(3, published.Playlist.EntryCount);
        Assert.Equal(_time.Now.UtcDateTime, published.Playlist.TimeLastPlayed);
        Assert.Equal(playlist.TimeModified, published.Playlist.TimeModified);
        Assert.Equal(source.Token, Assert.Single(_publisher.TokenList));
    }

    [Fact]
    public async Task Handle_ASecondCallInsideTheWindow_NeitherCommitsNorPublishes()
    {
        EntityPlaylist playlist = Seed();
        await _handler.Handle(new(UserID, playlist.ID), Token);
        DateTime stamp = Stored(playlist.ID).TimeLastPlayed ?? throw new InvalidOperationException("The stamp was not set.");
        int commits = _store.CommitCount;
        Advance(9);

        await _handler.Handle(new(UserID, playlist.ID), Token);

        Assert.Equal(commits, _store.CommitCount);
        Assert.Single(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Equal(stamp, Stored(playlist.ID).TimeLastPlayed);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(3600)]
    public async Task Handle_ACallAtOrAfterTheEndOfTheWindow_UpdatesAgain(int seconds)
    {
        EntityPlaylist playlist = Seed();
        await _handler.Handle(new(UserID, playlist.ID), Token);
        Advance(seconds);

        await _handler.Handle(new(UserID, playlist.ID), Token);

        Assert.Equal(2, _publisher.GetList<NotificationPlaylistUpdated>().Count);
        Assert.Equal(_time.Now.UtcDateTime, Stored(playlist.ID).TimeLastPlayed);
    }

    [Fact]
    public async Task Handle_AnotherUsersOrAnUnknownPlaylist_IsNotFound_AndNothingIsSavedOrPublished()
    {
        EntityPlaylist foreign = Seed(Guid.CreateVersion7());
        int commits = _store.CommitCount;

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => _handler.Handle(new(UserID, foreign.ID), Token).AsTask());
        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => _handler.Handle(new(UserID, Guid.CreateVersion7()), Token).AsTask());

        Assert.Null(Stored(foreign.ID).TimeLastPlayed);
        Assert.Equal(commits, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Handle_WhenTheCommitFails_PublishesNothing()
    {
        EntityPlaylist playlist = Seed();
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new(UserID, playlist.ID), Token).AsTask());

        Assert.Empty(_publisher.PublishedList);
    }
}
