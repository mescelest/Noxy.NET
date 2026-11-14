using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Requests.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class MediaServiceChangeTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeFileStorageService _storage = new();
    private readonly FakeMediaNotifier _notifier = new();
    private readonly FakePlaylistNotifier _playlistNotifier = new();
    private readonly FakeTimeProvider _time = new();
    private readonly FakeStore _store = new();
    private readonly MediaService _service;

    public MediaServiceChangeTest()
    {
        IOptions<ConfigStorage> storageOptions = Options.Create(new ConfigStorage { RootPath = "x", AllowedExtensionList = ["mp3"] });
        _service = new(new FakeUnitOfWorkFactory(_store), new MediaFileValidator(storageOptions), _storage, new FakeUploadSignal(), storageOptions, Options.Create(new ConfigUpload()), new UploadGate(), _notifier, _playlistNotifier, new ETagService(), _time, NullLogger<MediaService>.Instance);
    }

    private MediaFile Seed(Guid? userID = null, string title = "Song", DateTime? timeCreated = null, DateTime? timeModified = null)
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

    private Task<ResponseMediaFile> Update(MediaFile file, RequestUpdateMedia request, Guid? userID = null)
    {
        return _service.Update(userID ?? UserID, file.ID, request, TestContext.Current.CancellationToken);
    }

    private Task<MediaLibrary> Library(string? ifNoneMatch = null, Guid? userID = null)
    {
        return _service.GetMediaLibrary(userID ?? UserID, ifNoneMatch, TestContext.Current.CancellationToken);
    }

    #region -- Update --

    [Fact]
    public async Task Update_ChangesOnlyTheSentTags()
    {
        MediaFile file = Seed();

        ResponseMediaFile response = await Update(file, new() { Artist = new("New Artist") });

        Assert.Equal("New Artist", response.Artist);
        Assert.Equal("Album", response.Album);
        Assert.Equal("Song", response.Title);
        Assert.Equal("New Artist", Stored(file).Metadata?.Artist);
        Assert.Equal("Album", Stored(file).Metadata?.Album);
    }

    [Fact]
    public async Task Update_StoresEveryEditableTag()
    {
        MediaFile file = Seed();

        ResponseMediaFile response = await Update(file, new()
        {
            Title = new("T"),
            Artist = new("Ar"),
            Album = new("Al"),
            AlbumArtist = new("AA"),
            Subtitle = new("S"),
            Comment = new("C"),
            Genre = new("G"),
            TrackNumber = new(4),
            DiscNumber = new(2),
            Year = new(2024)
        });

        Assert.Equal(["T", "Ar", "Al", "AA", "S", "C", "G"], [response.Title, response.Artist, response.Album, response.AlbumArtist, response.Subtitle, response.Comment, response.Genre]);
        Assert.Equal([4, 2, 2024], [response.TrackNumber, response.DiscNumber, response.Year]);
        Assert.Equal(2024, Stored(file).Metadata?.Year);
    }

    [Fact]
    public async Task Update_NullClearsATag()
    {
        MediaFile file = Seed();

        ResponseMediaFile response = await Update(file, new() { Artist = new(null), Year = new(null) });

        Assert.Null(response.Artist);
        Assert.Null(response.Year);
        Assert.Null(Stored(file).Metadata?.Artist);
        Assert.Null(Stored(file).Metadata?.Year);
        Assert.Equal("Album", Stored(file).Metadata?.Album);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Update_EmptyOrWhitespaceNullableTag_ClearsIt(string value)
    {
        MediaFile file = Seed();

        ResponseMediaFile response = await Update(file, new() { Artist = new(value), Genre = new(value) });

        Assert.Null(response.Artist);
        Assert.Null(response.Genre);
        Assert.Null(Stored(file).Metadata?.Artist);
    }

    [Fact]
    public async Task Update_TextIsTrimmed()
    {
        MediaFile file = Seed();

        ResponseMediaFile response = await Update(file, new() { Title = new("  New Title  "), Album = new("  Other ") });

        Assert.Equal("New Title", response.Title);
        Assert.Equal("Other", response.Album);
    }

    [Fact]
    public async Task Update_BumpsTimeModifiedToTheClock_AndKeepsTimeCreated()
    {
        MediaFile file = Seed();
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);

        ResponseMediaFile response = await Update(file, new() { Title = new("Changed") });

        Assert.Equal(_time.Now.UtcDateTime, response.TimeModified);
        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), response.TimeCreated);
    }

    [Fact]
    public async Task Update_CommitsOnceAndPushesTheNewStateToTheOwner()
    {
        MediaFile file = Seed();

        ResponseMediaFile response = await Update(file, new() { Title = new("Changed") });

        Assert.Equal(1, _store.CommitCount);
        (Guid userID, ResponseMediaFile pushed) = Assert.Single(_notifier.UpdatedList);
        Assert.Equal(UserID, userID);
        Assert.Equal(file.ID, pushed.ID);
        Assert.Equal("Changed", pushed.Title);
        Assert.Equal(response.TimeModified, pushed.TimeModified);
    }

    [Fact]
    public async Task Update_NothingChanged_DoesNotCommitBumpOrPush_AndReturnsTheSong()
    {
        MediaFile file = Seed();
        DateTime before = file.TimeModified;
        _time.Now = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

        ResponseMediaFile same = await Update(file, new() { Title = new("Song"), Artist = new("Artist"), Year = new(1999) });
        ResponseMediaFile empty = await Update(file, new());

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_notifier.UpdatedList);
        Assert.Equal(before, Stored(file).TimeModified);
        Assert.Equal("Song", same.Title);
        Assert.Equal(file.ID, empty.ID);
    }

    [Fact]
    public async Task Update_AnotherUsersSong_ThrowsNotFound_AndPushesNothing()
    {
        MediaFile file = Seed();

        await Assert.ThrowsAsync<MediaNotFoundException>(() => Update(file, new() { Title = new("Hijacked") }, Guid.CreateVersion7()));

        Assert.Equal("Song", Stored(file).Metadata?.Title);
        Assert.Empty(_notifier.UpdatedList);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Update_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<MediaNotFoundException>(() => _service.Update(UserID, Guid.CreateVersion7(), new() { Title = new("x") }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Update_SongWithoutMetadata_ThrowsNotFound()
    {
        MediaFile file = Seed();
        file.Metadata = null;

        await Assert.ThrowsAsync<MediaNotFoundException>(() => Update(file, new() { Title = new("x") }));
    }

    [Fact]
    public async Task Update_NotifierThrows_StillReturnsTheSong()
    {
        MediaFile file = Seed();
        _notifier.Failure = new InvalidOperationException("no connection");

        ResponseMediaFile response = await Update(file, new() { Title = new("Changed") });

        Assert.Equal("Changed", response.Title);
        Assert.Equal("Changed", Stored(file).Metadata?.Title);
    }

    #endregion -- Update --

    #region -- Delete --

    [Fact]
    public async Task Delete_RemovesTheSongItsFileAndPushesDeleted()
    {
        MediaFile file = Seed();
        MediaFile kept = Seed();

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

        Assert.Equal([kept.ID], _store.MediaFileList.Select(x => x.ID));
        Assert.DoesNotContain(file.FilePath, _storage.Files.Keys);
        Assert.Contains(kept.FilePath, _storage.Files.Keys);
        Assert.Equal((UserID, file.ID), Assert.Single(_notifier.DeletedList));
    }

    [Fact]
    public async Task Delete_FileDeletionFails_StillSucceeds()
    {
        MediaFile file = Seed();
        _storage.DeleteFailure = new IOException("in use");

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

        Assert.Empty(_store.MediaFileList);
        Assert.Single(_notifier.DeletedList);
    }

    [Fact]
    public async Task Delete_FileAlreadyMissing_StillSucceeds()
    {
        MediaFile file = Seed();
        _storage.Files.Clear();

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

        Assert.Empty(_store.MediaFileList);
    }

    [Fact]
    public async Task Delete_AnotherUsersSong_ThrowsNotFound_AndKeepsTheFile()
    {
        MediaFile file = Seed();

        await Assert.ThrowsAsync<MediaNotFoundException>(() => _service.Delete(Guid.CreateVersion7(), file.ID, TestContext.Current.CancellationToken));

        Assert.Single(_store.MediaFileList);
        Assert.Contains(file.FilePath, _storage.Files.Keys);
        Assert.Empty(_notifier.DeletedList);
    }

    [Fact]
    public async Task Delete_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<MediaNotFoundException>(() => _service.Delete(UserID, Guid.CreateVersion7(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Delete_NotifierThrows_StillSucceeds()
    {
        MediaFile file = Seed();
        _notifier.Failure = new InvalidOperationException("no connection");

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

        Assert.Empty(_store.MediaFileList);
    }

    private Playlist SeedPlaylist(Guid userID, params MediaFile[] songList)
    {
        Playlist playlist = new()
        {
            UserID = userID,
            Name = "Mix",
            TimeCreated = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TimeModified = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        for (int index = 0; index < songList.Length; index++)
        {
            playlist.EntryList.Add(new() { PlaylistID = playlist.ID, MediaFileID = songList[index].ID, Position = index });
        }

        _store.PlaylistList.Add(playlist);
        return playlist;
    }

    private Playlist StoredPlaylist(Playlist playlist)
    {
        return _store.PlaylistList.Single(x => x.ID == playlist.ID);
    }

    [Fact]
    public async Task Delete_RemovesTheSongFromEveryPlaylistOfTheUser()
    {
        MediaFile file = Seed();
        MediaFile other = Seed();
        Playlist one = SeedPlaylist(UserID, file, other);
        Playlist two = SeedPlaylist(UserID, file);
        Playlist unrelated = SeedPlaylist(UserID, other);

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

        Assert.Equal([other.ID], StoredPlaylist(one).EntryList.Select(x => x.MediaFileID));
        Assert.Empty(StoredPlaylist(two).EntryList);
        Assert.Single(StoredPlaylist(unrelated).EntryList);
    }

    [Fact]
    public async Task Delete_SongTwiceInOnePlaylist_RemovesBothEntries()
    {
        MediaFile file = Seed();
        MediaFile other = Seed();
        Playlist playlist = SeedPlaylist(UserID, file, other, file);

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

        Assert.Equal([other.ID], StoredPlaylist(playlist).EntryList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task Delete_RenumbersThePlaylistsToDensePositions()
    {
        MediaFile file = Seed();
        MediaFile first = Seed();
        MediaFile second = Seed();
        Playlist playlist = SeedPlaylist(UserID, first, file, second, file);

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

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

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

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

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

        Assert.Equal(2, _playlistNotifier.UpdatedList.Count);
        Assert.All(_playlistNotifier.UpdatedList, x => Assert.Equal(UserID, x.UserID));
        Assert.Equal(1, _playlistNotifier.UpdatedList.Single(x => x.Playlist.ID == one.ID).Playlist.EntryCount);
        Assert.Equal(0, _playlistNotifier.UpdatedList.Single(x => x.Playlist.ID == two.ID).Playlist.EntryCount);
    }

    [Fact]
    public async Task Delete_AnotherUsersPlaylistsAreUntouched()
    {
        MediaFile file = Seed();
        Playlist foreign = SeedPlaylist(Guid.CreateVersion7(), file);

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

        Assert.Empty(_playlistNotifier.UpdatedList);
        Assert.Equal(new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), StoredPlaylist(foreign).TimeModified);
    }

    [Fact]
    public async Task Delete_SongInNoPlaylist_ChangesNoPlaylistAndPushesNoUpdate()
    {
        MediaFile file = Seed();
        MediaFile other = Seed();
        Playlist playlist = SeedPlaylist(UserID, other);

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

        Assert.Empty(_playlistNotifier.UpdatedList);
        Assert.Single(StoredPlaylist(playlist).EntryList);
    }

    [Fact]
    public async Task Delete_PlaylistNotifierThrows_StillSucceeds()
    {
        MediaFile file = Seed();
        SeedPlaylist(UserID, file);
        _playlistNotifier.Failure = new InvalidOperationException("no connection");

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

        Assert.Empty(_store.MediaFileList);
        Assert.Single(_notifier.DeletedList);
    }

    [Fact]
    public async Task Delete_StillCommitsOnce_ForTheSongAndItsPlaylists()
    {
        MediaFile file = Seed();
        SeedPlaylist(UserID, file);
        SeedPlaylist(UserID, file);
        int commits = _store.CommitCount;

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

        Assert.Equal(commits + 1, _store.CommitCount);
    }

    #endregion -- Delete --

    #region -- Library --

    [Fact]
    public async Task Library_ETagIsAQuotedLowercaseSha256()
    {
        Seed();

        MediaLibrary library = await Library();

        Assert.Matches("^\"[0-9a-f]{64}\"$", library.ETag);
    }

    [Fact]
    public async Task Library_EmptyLibrary_HasAStableETagAndAnEmptyList()
    {
        MediaLibrary first = await Library();
        MediaLibrary second = await Library();

        Assert.Equal(first.ETag, second.ETag);
        Assert.Empty(first.MediaFileList!);
    }

    [Fact]
    public async Task Library_SameState_GivesTheSameETag()
    {
        Seed();
        Seed();

        Assert.Equal((await Library()).ETag, (await Library()).ETag);
    }

    [Fact]
    public async Task Library_ChangesAfterAnEdit()
    {
        MediaFile file = Seed();
        string before = (await Library()).ETag;

        await Update(file, new() { Title = new("Changed") });

        Assert.NotEqual(before, (await Library()).ETag);
    }

    [Fact]
    public async Task Library_ChangesAfterADelete()
    {
        MediaFile file = Seed();
        Seed();
        string before = (await Library()).ETag;

        await _service.Delete(UserID, file.ID, TestContext.Current.CancellationToken);

        Assert.NotEqual(before, (await Library()).ETag);
    }

    [Fact]
    public async Task Library_ChangesAfterANewFile()
    {
        Seed();
        string before = (await Library()).ETag;

        Seed();

        Assert.NotEqual(before, (await Library()).ETag);
    }

    [Fact]
    public async Task Library_IsPerUser()
    {
        Seed();
        string before = (await Library()).ETag;

        Seed(Guid.CreateVersion7());

        Assert.Equal(before, (await Library()).ETag);
    }

    [Theory]
    [InlineData("{0}")]
    [InlineData("W/{0}")]
    [InlineData("\"other\", {0}")]
    [InlineData("{0} , \"other\"")]
    [InlineData("*")]
    public async Task Library_MatchingIfNoneMatch_ReturnsNoList(string template)
    {
        Seed();
        string eTag = (await Library()).ETag;

        MediaLibrary library = await Library(string.Format(template, eTag));

        Assert.Equal(eTag, library.ETag);
        Assert.Null(library.MediaFileList);
    }

    [Theory]
    [InlineData("\"other\"")]
    [InlineData("garbage")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Library_MismatchedOrMalformedIfNoneMatch_ReturnsTheList(string? ifNoneMatch)
    {
        Seed();
        Seed();

        MediaLibrary library = await Library(ifNoneMatch);

        Assert.Equal(2, library.MediaFileList?.Count);
    }

    [Fact]
    public async Task Library_ListIsNewestFirst_WithTags()
    {
        DateTime now = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        Seed(title: "Older", timeCreated: now.AddDays(-2));
        Seed(title: "Newer", timeCreated: now);

        MediaLibrary library = await Library();

        Assert.Equal(["Newer", "Older"], library.MediaFileList!.Select(x => x.Title));
        Assert.Equal("Artist", library.MediaFileList![0].Artist);
        Assert.Equal(1999, library.MediaFileList![0].Year);
    }

    [Fact]
    public async Task Library_NoOpEdit_DoesNotChangeTheETag()
    {
        MediaFile file = Seed();
        string before = (await Library()).ETag;

        await Update(file, new() { Title = new("Song") });

        Assert.Equal(before, (await Library()).ETag);
    }

    #endregion -- Library --
}
