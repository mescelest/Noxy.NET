using Noxy.NET.MediaPlayer.Application.Handlers.Playlist;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class HandlerPlaylistTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly HandlerPlaylist _handler;
    private DateTime _now = Start;

    public HandlerPlaylistTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new ETagService());
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private void Advance()
    {
        _now = _now.AddSeconds(1);
    }

    private MediaFile MediaFile()
    {
        MediaFile file = new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 3,
            ContentHash = new string('a', 64),
            UserID = UserID,
            TimeModified = _now
        };

        _store.MediaFileList.Add(file);
        return file;
    }

    private Playlist Seed(IEnumerable<Guid>? mediaFileIDList = null, string name = "Mix")
    {
        Playlist playlist = new()
        {
            UserID = UserID,
            Name = name,
            TimeCreated = _now,
            TimeModified = _now
        };

        Append(playlist, mediaFileIDList ?? []);

        _store.PlaylistList.Add(playlist);
        return playlist;
    }

    private void Append(Playlist playlist, IEnumerable<Guid> mediaFileIDList)
    {
        foreach (Guid mediaFileID in mediaFileIDList)
        {
            playlist.EntryList.Add(new() { PlaylistID = playlist.ID, MediaFileID = mediaFileID, Position = playlist.EntryList.Count });
        }

        playlist.TimeModified = _now;
    }

    private List<Guid> MediaFileIDList(int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => MediaFile().ID)];
    }

    private async Task<PlaylistView> View(Guid id, string? ifNoneMatch = null, Guid? userID = null)
    {
        return await _handler.Handle(new(userID ?? UserID, id, ifNoneMatch), Token);
    }

    [Fact]
    public async Task Handle_ReturnsEntriesByPosition()
    {
        List<Guid> mediaFileList = MediaFileIDList(3);
        Playlist playlist = Seed(mediaFileList);
        playlist.EntryList.Reverse();

        PlaylistView view = await View(playlist.ID);

        Assert.NotNull(view.Detail);
        Assert.Equal(mediaFileList, view.Detail.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1, 2], view.Detail.EntryList.Select(x => x.Position));
        Assert.Equal(3, view.Detail.EntryCount);
    }

    [Fact]
    public async Task Handle_MatchingIfNoneMatch_ReturnsNoDetail()
    {
        Playlist playlist = Seed();
        string eTag = (await View(playlist.ID)).ETag;

        PlaylistView view = await View(playlist.ID, eTag);

        Assert.Equal(eTag, view.ETag);
        Assert.Null(view.Detail);
    }

    [Fact]
    public async Task Handle_ETagChangesWhenItsEntriesChange_AndNotWhenAnotherPlaylistChanges()
    {
        Playlist playlist = Seed();
        Playlist other = Seed(name: "Other");
        string before = (await View(playlist.ID)).ETag;
        Advance();

        Append(other, MediaFileIDList(1));
        Assert.Equal(before, (await View(playlist.ID)).ETag);

        Append(playlist, MediaFileIDList(1));
        Assert.NotEqual(before, (await View(playlist.ID)).ETag);
    }

    [Fact]
    public async Task Handle_AnotherUsersPlaylist_ThrowsNotFound()
    {
        Playlist playlist = Seed();

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => View(playlist.ID, userID: Guid.CreateVersion7()));
    }

    [Fact]
    public async Task Handle_UnknownPlaylist_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => View(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task Handle_WithAnIfNoneMatch_UnknownOrAnotherUsersPlaylist_ThrowsNotFound()
    {
        Playlist playlist = Seed();
        string eTag = (await View(playlist.ID)).ETag;

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => View(playlist.ID, eTag, Guid.CreateVersion7()));
        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => View(Guid.CreateVersion7(), eTag));
    }

    [Fact]
    public async Task Handle_ReturnsEntriesWithTheirMediaFileMetadata()
    {
        MediaFile mediaFile = MediaFile();
        mediaFile.Metadata = new() { Title = "Hello", Artist = "Someone", MediaFileID = mediaFile.ID };
        Playlist playlist = Seed([mediaFile.ID]);

        PlaylistView view = await View(playlist.ID);

        Assert.NotNull(view.Detail);
        ResponsePlaylistEntry entry = Assert.Single(view.Detail.EntryList);
        Assert.Equal(mediaFile.ID, entry.MediaFileID);
        Assert.Equal(mediaFile.ID, entry.MediaFile.ID);
        Assert.Equal("Hello", entry.MediaFile.Title);
        Assert.Equal("Someone", entry.MediaFile.Artist);
        Assert.Equal(mediaFile.ContentHash, entry.MediaFile.ContentHash);
    }

    [Fact]
    public async Task Handle_AfterAMediaFilesMetadataChanged_NoLongerMatchesTheOldETag()
    {
        MediaFile mediaFile = MediaFile();
        Playlist playlist = Seed([mediaFile.ID]);
        string eTag = (await View(playlist.ID)).ETag;
        Advance();
        mediaFile.TimeModified = _now;

        PlaylistView view = await View(playlist.ID, eTag);

        Assert.NotNull(view.Detail);
        Assert.NotEqual(eTag, view.ETag);
    }

    [Fact]
    public async Task Handle_WhenNothingChanged_ReturnsNullDetail()
    {
        Playlist playlist = Seed(MediaFileIDList(2));
        string eTag = (await View(playlist.ID)).ETag;

        PlaylistView view = await View(playlist.ID, eTag);

        Assert.Null(view.Detail);
        Assert.Equal(eTag, view.ETag);
    }

    [Fact]
    public async Task Handle_SameMediaFileTwice_ReturnsBothEntriesAndAnETag()
    {
        Guid mediaFileID = MediaFile().ID;
        Playlist playlist = Seed([mediaFileID, mediaFileID]);

        PlaylistView view = await View(playlist.ID);

        Assert.NotNull(view.Detail);
        Assert.Equal(2, view.Detail.EntryList.Count);
        Assert.NotEmpty(view.ETag);
        Assert.Null((await View(playlist.ID, view.ETag)).Detail);
    }

    [Fact]
    public async Task Handle_EmptyPlaylist_ReturnsADetailWithAnETag()
    {
        Playlist playlist = Seed();

        PlaylistView view = await View(playlist.ID);

        Assert.NotNull(view.Detail);
        Assert.Empty(view.Detail.EntryList);
        Assert.NotEmpty(view.ETag);
        Assert.Null((await View(playlist.ID, view.ETag)).Detail);
    }
}
