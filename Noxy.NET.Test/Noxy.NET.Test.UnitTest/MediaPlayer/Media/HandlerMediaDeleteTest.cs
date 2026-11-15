using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using Noxy.NET.MediaPlayer.Application.Handlers.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
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

    private MediaFile Seed(Guid? userID = null, string title = "Media", DateTime? timeCreated = null, DateTime? timeModified = null)
    {
        byte[] bytes = [1, 2, 3];
        MediaFile file = new()
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

    private MediaFile Stored(MediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    #region -- Delete --

    [Fact]
    public async Task Delete_RemovesTheMediaFileItsFileAndPushesDeleted()
    {
        MediaFile file = Seed();
        MediaFile kept = Seed();

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
        MediaFile file = Seed();
        _storage.DeleteFailure = new IOException("in use");

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Empty(_store.MediaFileList);
        Assert.Single(_publisher.GetList<NotificationMediaDeleted>());
    }

    [Fact]
    public async Task Delete_FileAlreadyMissing_StillSucceeds()
    {
        MediaFile file = Seed();
        _storage.Files.Clear();

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Empty(_store.MediaFileList);
    }

    [Fact]
    public async Task Delete_AnotherUsersMediaFile_ThrowsNotFound_AndKeepsTheFile()
    {
        MediaFile file = Seed();

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

    private Playlist SeedPlaylist(Guid userID, params MediaFile[] mediaFileList)
    {
        Playlist playlist = new()
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

    private Playlist StoredPlaylist(Playlist playlist)
    {
        return _store.PlaylistList.Single(x => x.ID == playlist.ID);
    }

    [Fact]
    public async Task Delete_RemovesTheMediaFileFromEveryPlaylistOfTheUser()
    {
        MediaFile file = Seed();
        MediaFile other = Seed();
        Playlist one = SeedPlaylist(UserID, file, other);
        Playlist two = SeedPlaylist(UserID, file);
        Playlist unrelated = SeedPlaylist(UserID, other);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal([other.ID], StoredPlaylist(one).EntryList.Select(x => x.MediaFileID));
        Assert.Empty(StoredPlaylist(two).EntryList);
        Assert.Single(StoredPlaylist(unrelated).EntryList);
    }

    [Fact]
    public async Task Delete_MediaFileTwiceInOnePlaylist_RemovesBothEntries()
    {
        MediaFile file = Seed();
        MediaFile other = Seed();
        Playlist playlist = SeedPlaylist(UserID, file, other, file);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal([other.ID], StoredPlaylist(playlist).EntryList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task Delete_RenumbersThePlaylistsToDensePositions()
    {
        MediaFile file = Seed();
        MediaFile first = Seed();
        MediaFile second = Seed();
        Playlist playlist = SeedPlaylist(UserID, first, file, second, file);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        List<PlaylistEntry> left = [.. StoredPlaylist(playlist).EntryList.OrderBy(x => x.Position)];
        Assert.Equal([first.ID, second.ID], left.Select(x => x.MediaFileID));
        Assert.Equal([0, 1], left.Select(x => x.Position));
    }

    [Fact]
    public async Task Delete_BumpsTimeModifiedOfAffectedPlaylistsOnly()
    {
        MediaFile file = Seed();
        MediaFile other = Seed();
        Playlist affected = SeedPlaylist(UserID, file);
        Playlist untouched = SeedPlaylist(UserID, other);
        DateTime before = untouched.TimeModified;
        _time.Now = _time.Now.AddHours(1);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal(_time.Now.UtcDateTime, StoredPlaylist(affected).TimeModified);
        Assert.Equal(before, StoredPlaylist(untouched).TimeModified);
    }

    [Fact]
    public async Task Delete_PushesOneUpdatePerAffectedPlaylistWithTheNewCount()
    {
        MediaFile file = Seed();
        MediaFile other = Seed();
        Playlist one = SeedPlaylist(UserID, file, other);
        Playlist two = SeedPlaylist(UserID, file, file);
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
        MediaFile file = Seed();
        Playlist foreign = SeedPlaylist(Guid.CreateVersion7(), file);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Empty(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Equal(new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), StoredPlaylist(foreign).TimeModified);
    }

    [Fact]
    public async Task Delete_MediaFileInNoPlaylist_ChangesNoPlaylistAndPushesNoUpdate()
    {
        MediaFile file = Seed();
        MediaFile other = Seed();
        Playlist playlist = SeedPlaylist(UserID, other);

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Empty(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Single(StoredPlaylist(playlist).EntryList);
    }

    [Fact]
    public async Task Delete_StillCommitsOnce_ForTheMediaFileAndItsPlaylists()
    {
        MediaFile file = Seed();
        SeedPlaylist(UserID, file);
        SeedPlaylist(UserID, file);
        int commits = _store.CommitCount;

        await _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken);

        Assert.Equal(commits + 1, _store.CommitCount);
    }

    [Fact]
    public async Task Delete_PublishesMediaDeletedFirstThenOnePlaylistUpdatedPerAffectedPlaylist_AfterTheFileIsRemoved()
    {
        MediaFile file = Seed();
        Playlist one = SeedPlaylist(UserID, file);
        Playlist two = SeedPlaylist(UserID, file);
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
        MediaFile file = Seed();
        SeedPlaylist(UserID, file);
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new(UserID, file.ID), TestContext.Current.CancellationToken).AsTask());

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Delete_PassesTheCancellationTokenToEveryPublish()
    {
        MediaFile file = Seed();
        SeedPlaylist(UserID, file);
        using CancellationTokenSource source = new();

        await _handler.Handle(new(UserID, file.ID), source.Token);

        Assert.Equal(2, _publisher.TokenList.Count);
        Assert.All(_publisher.TokenList, x => Assert.Equal(source.Token, x));
    }

    #endregion -- Delete --
}
