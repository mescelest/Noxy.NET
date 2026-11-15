using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using Noxy.NET.MediaPlayer.Application.Handlers.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Playback;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class HandlerMediaDeleteTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeFileStorageService _storage = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly FakeStore _store = new();
    private readonly HandlerMediaDelete _handler;

    public HandlerMediaDeleteTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), _storage, _publisher, _time, NullLogger<HandlerMediaDelete>.Instance);
    }

    private EntityMediaFile Seed(Guid? userID = null, string title = "Media", DateTime? timeCreated = null, DateTime? timeModified = null)
    {
        byte[] bytes = [1, 2, 3];
        EntityMediaFile file = new()
        {
            TimeCreated = timeCreated ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TimeModified = timeModified ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = bytes.Length,
            ContentHash = Convert.ToHexStringLower(SHA256.HashData(bytes)),
            UserID = userID ?? UserID
        };
        file.Metadata = new() { Title = title, Artist = "Artist", Album = "Album", Genre = "Genre", Year = 1999, MediaFileID = file.ID };

        _storage.Files[file.FilePath] = bytes;
        _store.MediaFileList.Add(file);
        return file;
    }

    #region -- Delete --

    [Fact]
    public async Task Delete_RemovesTheMediaFileItsFileAndPushesDeleted()
    {
        EntityMediaFile file = Seed();
        EntityMediaFile kept = Seed();

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal([kept.ID], _store.MediaFileList.Select(x => x.ID));
        Assert.DoesNotContain(file.FilePath, _storage.Files.Keys);
        Assert.Contains(kept.FilePath, _storage.Files.Keys);
        NotificationMediaDeleted deleted = Assert.Single(_publisher.GetList<NotificationMediaDeleted>());
        Assert.Equal(UserID, deleted.UserID);
        Assert.Equal(file.ID, deleted.ID);
    }

    [Fact]
    public async Task Delete_FileDeletionFails_StillSucceeds()
    {
        EntityMediaFile file = Seed();
        _storage.DeleteFailure = new IOException("in use");

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Empty(_store.MediaFileList);
        Assert.Single(_publisher.GetList<NotificationMediaDeleted>());
    }

    [Fact]
    public async Task Delete_FileAlreadyMissing_StillSucceeds()
    {
        EntityMediaFile file = Seed();
        _storage.Files.Clear();

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Empty(_store.MediaFileList);
    }

    [Fact]
    public async Task Delete_AnotherUsersMediaFile_ThrowsNotFound_AndKeepsTheFile()
    {
        EntityMediaFile file = Seed();

        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new(Guid.CreateVersion7(), file.ID), TestContext.Current.CancellationToken).AsTask());

        Assert.Single(_store.MediaFileList);
        Assert.Contains(file.FilePath, _storage.Files.Keys);
        Assert.Empty(_publisher.GetList<NotificationMediaDeleted>());
    }

    [Fact]
    public async Task Delete_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new(UserID, Guid.CreateVersion7()), TestContext.Current.CancellationToken).AsTask());
    }

    private EntityPlaylist SeedPlaylist(Guid userID, params EntityMediaFile[] mediaFileList)
    {
        EntityPlaylist playlist = new()
        {
            UserID = userID,
            Name = "Mix",
            TimeCreated = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TimeModified = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        for (int index = 0; index < mediaFileList.Length; index++)
        {
            playlist.EntryList.Add(new() { PlaylistID = playlist.ID, MediaFileID = mediaFileList[index].ID, Position = index });
        }

        _store.PlaylistList.Add(playlist);
        return playlist;
    }

    private EntityPlaylist StoredPlaylist(EntityPlaylist playlist)
    {
        return _store.PlaylistList.Single(x => x.ID == playlist.ID);
    }

    [Fact]
    public async Task Delete_RemovesTheMediaFileFromEveryPlaylistOfTheUser()
    {
        EntityMediaFile file = Seed();
        EntityMediaFile other = Seed();
        EntityPlaylist one = SeedPlaylist(UserID, file, other);
        EntityPlaylist two = SeedPlaylist(UserID, file);
        EntityPlaylist unrelated = SeedPlaylist(UserID, other);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal([other.ID], StoredPlaylist(one).EntryList.Select(x => x.MediaFileID));
        Assert.Empty(StoredPlaylist(two).EntryList);
        Assert.Single(StoredPlaylist(unrelated).EntryList);
    }

    [Fact]
    public async Task Delete_MediaFileTwiceInOnePlaylist_RemovesBothEntries()
    {
        EntityMediaFile file = Seed();
        EntityMediaFile other = Seed();
        EntityPlaylist playlist = SeedPlaylist(UserID, file, other, file);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal([other.ID], StoredPlaylist(playlist).EntryList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task Delete_RenumbersThePlaylistsToDensePositions()
    {
        EntityMediaFile file = Seed();
        EntityMediaFile first = Seed();
        EntityMediaFile second = Seed();
        EntityPlaylist playlist = SeedPlaylist(UserID, first, file, second, file);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        List<EntityPlaylistEntry> left = [.. StoredPlaylist(playlist).EntryList.OrderBy(x => x.Position)];
        Assert.Equal([first.ID, second.ID], left.Select(x => x.MediaFileID));
        Assert.Equal([0, 1], left.Select(x => x.Position));
    }

    [Fact]
    public async Task Delete_BumpsTimeModifiedOfAffectedPlaylistsOnly()
    {
        EntityMediaFile file = Seed();
        EntityMediaFile other = Seed();
        EntityPlaylist affected = SeedPlaylist(UserID, file);
        EntityPlaylist untouched = SeedPlaylist(UserID, other);
        DateTime before = untouched.TimeModified;
        _time.Now = _time.Now.AddHours(1);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal(_time.Now.UtcDateTime, StoredPlaylist(affected).TimeModified);
        Assert.Equal(before, StoredPlaylist(untouched).TimeModified);
    }

    [Fact]
    public async Task Delete_PushesOneUpdatePerAffectedPlaylistWithTheNewCount()
    {
        EntityMediaFile file = Seed();
        EntityMediaFile other = Seed();
        EntityPlaylist one = SeedPlaylist(UserID, file, other);
        EntityPlaylist two = SeedPlaylist(UserID, file, file);
        SeedPlaylist(UserID, other);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal(2, _publisher.GetList<NotificationPlaylistUpdated>().Count);
        Assert.All(_publisher.GetList<NotificationPlaylistUpdated>(), x => Assert.Equal(UserID, x.UserID));
        Assert.Equal(1, _publisher.GetList<NotificationPlaylistUpdated>().Single(x => x.Playlist.ID == one.ID).Playlist.EntryCount);
        Assert.Equal(0, _publisher.GetList<NotificationPlaylistUpdated>().Single(x => x.Playlist.ID == two.ID).Playlist.EntryCount);
    }

    [Fact]
    public async Task Delete_AnotherUsersPlaylistsAreUntouched()
    {
        EntityMediaFile file = Seed();
        EntityPlaylist foreign = SeedPlaylist(Guid.CreateVersion7(), file);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Empty(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Equal(new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), StoredPlaylist(foreign).TimeModified);
    }

    [Fact]
    public async Task Delete_MediaFileInNoPlaylist_ChangesNoPlaylistAndPushesNoUpdate()
    {
        EntityMediaFile file = Seed();
        EntityMediaFile other = Seed();
        EntityPlaylist playlist = SeedPlaylist(UserID, other);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Empty(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Single(StoredPlaylist(playlist).EntryList);
    }

    [Fact]
    public async Task Delete_StillCommitsOnce_ForTheMediaFileAndItsPlaylists()
    {
        EntityMediaFile file = Seed();
        SeedPlaylist(UserID, file);
        SeedPlaylist(UserID, file);
        int commits = _store.CommitCount;

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal(commits + 1, _store.CommitCount);
    }

    [Fact]
    public async Task Delete_PublishesMediaDeletedFirstThenOnePlaylistUpdatedPerAffectedPlaylist_AfterTheFileIsRemoved()
    {
        EntityMediaFile file = Seed();
        EntityPlaylist one = SeedPlaylist(UserID, file);
        EntityPlaylist two = SeedPlaylist(UserID, file);
        List<bool> fileGoneAtPublishList = [];
        _publisher.OnPublish = _ => fileGoneAtPublishList.Add(!_storage.Files.ContainsKey(file.FilePath));

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal(3, _publisher.PublishedList.Count);
        Assert.Equal(file.ID, Assert.IsType<NotificationMediaDeleted>(_publisher.PublishedList[0]).ID);
        Assert.Equal(new[] { one.ID, two.ID }.Order(), _publisher.GetList<NotificationPlaylistUpdated>().Select(x => x.Playlist.ID).Order());
        Assert.IsType<NotificationPlaylistUpdated>(_publisher.PublishedList[1]);
        Assert.IsType<NotificationPlaylistUpdated>(_publisher.PublishedList[2]);
        Assert.All(fileGoneAtPublishList, Assert.True);
    }

    [Fact]
    public async Task Delete_WhenTheCommitFails_PublishesNothing()
    {
        EntityMediaFile file = Seed();
        SeedPlaylist(UserID, file);
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken).AsTask());

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Delete_PassesTheCancellationTokenToEveryPublish()
    {
        EntityMediaFile file = Seed();
        SeedPlaylist(UserID, file);
        using CancellationTokenSource source = new();

        await _handler.Handle(new(UserID, file.ID), source.Token);

        Assert.Equal(2, _publisher.TokenList.Count);
        Assert.All(_publisher.TokenList, x => Assert.Equal(source.Token, x));
    }

    #endregion -- Delete --

    #region -- Playback session --

    private EntityPlaybackSession SeedSession(Guid userID, List<Guid> queue, int? index, double position = 30)
    {
        EntityPlaybackSession session = new()
        {
            UserID = userID,
            QueueMediaFileIDList = queue,
            CurrentIndex = index,
            PositionSeconds = position,
            TimeModified = _time.Now.UtcDateTime
        };

        _store.PlaybackSessionList.Add(session);
        return session;
    }

    [Fact]
    public async Task Delete_RemovesTheMediaFileFromTheSessionQueue_AdjustsTheIndex_AndPushesTheSession()
    {
        EntityMediaFile first = Seed();
        EntityMediaFile removed = Seed();
        EntityMediaFile last = Seed();
        EntityPlaybackSession session = SeedSession(UserID, [first.ID, removed.ID, last.ID], 2);
        DateTime before = session.TimeModified;
        _time.Now = _time.Now.AddMinutes(1);

        await _handler.Handle(new(UserID, removed.ID), TestContext.Current.CancellationToken);

        EntityPlaybackSession stored = Assert.Single(_store.PlaybackSessionList);
        Assert.Equal([first.ID, last.ID], stored.QueueMediaFileIDList);
        Assert.Equal(1, stored.CurrentIndex);
        Assert.Equal(30, stored.PositionSeconds);
        Assert.True(stored.TimeModified > before);
        NotificationPlaybackSessionUpdated pushed = Assert.Single(_publisher.GetList<NotificationPlaybackSessionUpdated>());
        Assert.Equal(UserID, pushed.UserID);
        Assert.Equal([first.ID, last.ID], pushed.Session.QueueMediaFileIDList);
        Assert.Equal(1, pushed.Session.CurrentIndex);
    }

    [Fact]
    public async Task Delete_OfTheCurrentMediaFile_ClearsTheIndexAndThePosition()
    {
        EntityMediaFile current = Seed();
        EntityMediaFile other = Seed();
        SeedSession(UserID, [other.ID, current.ID], 1, 77);

        await _handler.Handle(new(UserID, current.ID), TestContext.Current.CancellationToken);

        EntityPlaybackSession stored = Assert.Single(_store.PlaybackSessionList);
        Assert.Equal([other.ID], stored.QueueMediaFileIDList);
        Assert.Null(stored.CurrentIndex);
        Assert.Equal(0, stored.PositionSeconds);
    }

    [Fact]
    public async Task Delete_OfTheLastMediaFileInTheQueue_RemovesTheSession_AndPushesCleared()
    {
        EntityMediaFile file = Seed();
        SeedSession(UserID, [file.ID, file.ID], 0);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Empty(_store.PlaybackSessionList);
        Assert.Equal(UserID, Assert.Single(_publisher.GetList<NotificationPlaybackSessionCleared>()).UserID);
        Assert.Empty(_publisher.GetList<NotificationPlaybackSessionUpdated>());
    }

    [Fact]
    public async Task Delete_OfAMediaFileNotInTheQueue_LeavesTheSessionAndPushesNoSession()
    {
        EntityMediaFile file = Seed();
        EntityMediaFile other = Seed();
        EntityPlaybackSession session = SeedSession(UserID, [other.ID], 0);
        DateTime modified = session.TimeModified;
        _time.Now = _time.Now.AddMinutes(1);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        EntityPlaybackSession stored = Assert.Single(_store.PlaybackSessionList);
        Assert.Equal([other.ID], stored.QueueMediaFileIDList);
        Assert.Equal(modified, stored.TimeModified);
        Assert.Empty(_publisher.GetList<NotificationPlaybackSessionUpdated>());
        Assert.Empty(_publisher.GetList<NotificationPlaybackSessionCleared>());
    }

    [Fact]
    public async Task Delete_LeavesAnotherUsersSessionAlone()
    {
        EntityMediaFile file = Seed();
        Guid other = Guid.CreateVersion7();
        SeedSession(other, [file.ID], 0);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal([file.ID], Assert.Single(_store.PlaybackSessionList).QueueMediaFileIDList);
        Assert.Empty(_publisher.GetList<NotificationPlaybackSessionUpdated>());
    }

    [Fact]
    public async Task Delete_ChangesTheSessionInTheSameCommitAsTheMediaFile_AndPublishesItLast()
    {
        EntityMediaFile file = Seed();
        EntityMediaFile other = Seed();
        SeedPlaylist(UserID, file);
        SeedSession(UserID, [file.ID, other.ID], 1);
        int commits = _store.CommitCount;

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal(commits + 1, _store.CommitCount);
        Assert.Equal([typeof(NotificationMediaDeleted), typeof(NotificationPlaylistUpdated), typeof(NotificationPlaybackSessionUpdated)], _publisher.PublishedList.Select(x => x.GetType()));
    }

    [Fact]
    public async Task Delete_WhenTheCommitFails_LeavesTheSessionAndPublishesNothing()
    {
        EntityMediaFile file = Seed();
        SeedSession(UserID, [file.ID], 0);
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken).AsTask());

        Assert.Equal([file.ID], Assert.Single(_store.PlaybackSessionList).QueueMediaFileIDList);
        Assert.Empty(_publisher.PublishedList);
    }

    #endregion -- Playback session --
}
