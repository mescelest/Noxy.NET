using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

// The tests every metadata store must pass. A store's test class inherits this and says how to make one.
public abstract class MetadataStoreContract
{
    private static readonly DateTime Start = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    protected abstract IMetadataStore CreateStore();

    private static ResponseMediaFile MediaFile(string title = "Media", Guid? id = null)
    {
        return new()
        {
            ID = id ?? Guid.CreateVersion7(),
            Title = title,
            ContentType = "audio/mpeg",
            SizeBytes = 1234,
            ContentHash = new string('a', 64),
            TimeCreated = Start,
            TimeModified = Start
        };
    }

    private static ResponseMediaFile FullMediaFile()
    {
        return new()
        {
            ID = Guid.CreateVersion7(),
            Title = "Title with ünïcödé and \"quotes\"",
            ContentType = "audio/flac",
            SizeBytes = 9_876_543_210,
            ContentHash = new string('b', 64),
            TimeCreated = Start,
            TimeModified = Start.AddMinutes(5),
            Artist = "Artist; Other",
            Album = "Album",
            AlbumArtist = "Album Artist",
            Subtitle = "Subtitle",
            Comment = "A comment\nwith a line break",
            Genre = "Genre",
            TrackNumber = 3,
            DiscNumber = 2,
            Year = 1999,
            Duration = TimeSpan.FromSeconds(215.5),
            ArtistIDList = [Guid.CreateVersion7(), Guid.CreateVersion7()],
            AlbumID = Guid.CreateVersion7(),
            AlbumArtistIDList = [Guid.CreateVersion7()],
            GenreIDList = [Guid.CreateVersion7()]
        };
    }

    private static ResponsePlaylist Playlist(string name = "Mix", Guid? id = null, DateTime? lastPlayed = null)
    {
        return new()
        {
            ID = id ?? Guid.CreateVersion7(),
            Name = name,
            AllowDuplicates = true,
            EntryCount = 2,
            TimeCreated = Start,
            TimeModified = Start,
            TimeLastPlayed = lastPlayed
        };
    }

    private static ResponsePlaylistDetail Detail(Guid id, ResponseMediaFile? repeated = null)
    {
        ResponseMediaFile first = repeated ?? MediaFile("First");
        return new()
        {
            ID = id,
            Name = "Mix",
            AllowDuplicates = true,
            EntryCount = 3,
            TimeCreated = Start,
            TimeModified = Start,
            TimeLastPlayed = Start.AddHours(1),
            EntryList =
            [
                new() { ID = Guid.CreateVersion7(), MediaFileID = first.ID, Position = 0, MediaFile = first },
                new() { ID = Guid.CreateVersion7(), MediaFileID = first.ID, Position = 1, MediaFile = first },
                new() { ID = Guid.CreateVersion7(), MediaFileID = Guid.CreateVersion7(), Position = 2, MediaFile = MediaFile("Third") }
            ]
        };
    }

    // Two values are the same when they serialize to the same JSON, which is what the API sends and the store keeps.
    private static void AssertSame<T>(T expected, T? actual)
    {
        Assert.Equal(JsonSerializer.Serialize(expected, JsonSerializerOptions.Web), JsonSerializer.Serialize(actual, JsonSerializerOptions.Web));
    }

    [Fact]
    public async Task AnEmptyStore_HasNoListsNoETagsAndNoDetail()
    {
        IMetadataStore store = CreateStore();

        Assert.Empty(await store.GetMediaFileList());
        Assert.Empty(await store.GetPlaylistList());
        Assert.Empty(await store.GetHeldPlaylistIDList());
        Assert.Null(await store.GetLibraryETag());
        Assert.Null(await store.GetPlaylistListETag());
        Assert.Null(await store.GetPlaylistDetail(Guid.CreateVersion7()));
    }

    #region -- Media files --

    [Fact]
    public async Task ReplaceMediaFileList_ReplacesRatherThanMerges_AndKeepsTheGivenOrder()
    {
        IMetadataStore store = CreateStore();
        await store.ReplaceMediaFileList([MediaFile("Old")]);
        ResponseMediaFile b = MediaFile("B");
        ResponseMediaFile a = MediaFile("A");

        await store.ReplaceMediaFileList([b, a]);

        Assert.Equal([b.ID, a.ID], (await store.GetMediaFileList()).Select(x => x.ID));
    }

    [Fact]
    public async Task ReplaceMediaFileList_WithAnEmptyList_EmptiesTheStore()
    {
        IMetadataStore store = CreateStore();
        await store.ReplaceMediaFileList([MediaFile()]);

        await store.ReplaceMediaFileList([]);

        Assert.Empty(await store.GetMediaFileList());
    }

    [Fact]
    public async Task SaveMediaFile_InsertsANewOneAtTheEnd_AndReplacesAnExistingOneInPlace()
    {
        IMetadataStore store = CreateStore();
        ResponseMediaFile first = MediaFile("First");
        ResponseMediaFile second = MediaFile("Second");
        await store.ReplaceMediaFileList([first, second]);

        ResponseMediaFile third = MediaFile("Third");
        await store.SaveMediaFile(third);
        await store.SaveMediaFile(MediaFile("First renamed", first.ID));

        List<ResponseMediaFile> list = await store.GetMediaFileList();
        Assert.Equal([first.ID, second.ID, third.ID], list.Select(x => x.ID));
        Assert.Equal("First renamed", list[0].Title);
    }

    [Fact]
    public async Task DeleteMediaFile_RemovesIt_AndAnUnknownIDIsHarmless()
    {
        IMetadataStore store = CreateStore();
        ResponseMediaFile kept = MediaFile("Kept");
        ResponseMediaFile gone = MediaFile("Gone");
        await store.ReplaceMediaFileList([kept, gone]);

        await store.DeleteMediaFile(gone.ID);
        await store.DeleteMediaFile(Guid.CreateVersion7());

        Assert.Equal([kept.ID], (await store.GetMediaFileList()).Select(x => x.ID));
    }

    [Fact]
    public async Task GetMediaFile_ReturnsThatMediaFile_AndNullForAnUnknownID()
    {
        IMetadataStore store = CreateStore();
        ResponseMediaFile first = MediaFile("First");
        ResponseMediaFile second = MediaFile("Second");
        await store.ReplaceMediaFileList([first, second]);

        AssertSame(second, await store.GetMediaFile(second.ID));
        Assert.Null(await store.GetMediaFile(Guid.CreateVersion7()));

        await store.DeleteMediaFile(second.ID);

        Assert.Null(await store.GetMediaFile(second.ID));
    }

    [Fact]
    public async Task EveryFieldOfAMediaFile_ComesBackEqual_WhetherSetOrNull()
    {
        IMetadataStore store = CreateStore();
        ResponseMediaFile full = FullMediaFile();
        ResponseMediaFile minimal = MediaFile("Minimal");

        await store.ReplaceMediaFileList([full, minimal]);

        List<ResponseMediaFile> list = await store.GetMediaFileList();
        AssertSame(full, list[0]);
        AssertSame(minimal, list[1]);
        Assert.Equal(DateTimeKind.Utc, list[0].TimeCreated.Kind);
    }

    [Fact]
    public async Task ReplaceMediaFileList_With5000Items_ReturnsAll5000InTheGivenOrder()
    {
        IMetadataStore store = CreateStore();
        List<ResponseMediaFile> big = [.. Enumerable.Range(0, 5000).Select(index => MediaFile($"Media {index}"))];

        await store.ReplaceMediaFileList(big);

        Assert.Equal(big.Select(x => x.ID), (await store.GetMediaFileList()).Select(x => x.ID));
    }

    #endregion -- Media files --

    #region -- Playlists --

    [Fact]
    public async Task ReplacePlaylistList_ReplacesRatherThanMerges_AndKeepsTheGivenOrder()
    {
        IMetadataStore store = CreateStore();
        await store.ReplacePlaylistList([Playlist("Old")]);
        ResponsePlaylist b = Playlist("B");
        ResponsePlaylist a = Playlist("A");

        await store.ReplacePlaylistList([b, a]);

        Assert.Equal([b.ID, a.ID], (await store.GetPlaylistList()).Select(x => x.ID));
    }

    [Fact]
    public async Task SavePlaylist_InsertsOrReplacesByID_AndDeletePlaylistOfAnUnknownIDIsHarmless()
    {
        IMetadataStore store = CreateStore();
        ResponsePlaylist first = Playlist("First");
        await store.ReplacePlaylistList([first]);

        await store.SavePlaylist(Playlist("First renamed", first.ID));
        await store.SavePlaylist(Playlist("Second"));
        await store.DeletePlaylist(Guid.CreateVersion7());

        List<ResponsePlaylist> list = await store.GetPlaylistList();
        Assert.Equal(["First renamed", "Second"], list.Select(x => x.Name));
    }

    [Fact]
    public async Task EveryFieldOfAPlaylist_ComesBackEqual_WithAndWithoutALastPlayedTime()
    {
        IMetadataStore store = CreateStore();
        ResponsePlaylist never = Playlist("Never played");
        ResponsePlaylist played = Playlist("Played", lastPlayed: Start.AddDays(2));

        await store.ReplacePlaylistList([never, played]);

        List<ResponsePlaylist> list = await store.GetPlaylistList();
        AssertSame(never, list[0]);
        AssertSame(played, list[1]);
        Assert.Null(list[0].TimeLastPlayed);
        Assert.Equal(DateTimeKind.Utc, list[1].TimeLastPlayed?.Kind);
    }

    [Fact]
    public async Task DeletePlaylist_AlsoRemovesTheHeldDetail_AndItsIDFromTheHeldList()
    {
        IMetadataStore store = CreateStore();
        ResponsePlaylist playlist = Playlist();
        await store.ReplacePlaylistList([playlist]);
        await store.SavePlaylistDetail(Detail(playlist.ID));

        await store.DeletePlaylist(playlist.ID);

        Assert.Empty(await store.GetPlaylistList());
        Assert.Null(await store.GetPlaylistDetail(playlist.ID));
        Assert.Empty(await store.GetHeldPlaylistIDList());
    }

    [Fact]
    public async Task ReplacePlaylistList_DropsTheDetailOfAPlaylistThatIsNoLongerInTheList_AndKeepsTheOthers()
    {
        IMetadataStore store = CreateStore();
        ResponsePlaylist kept = Playlist("Kept");
        ResponsePlaylist gone = Playlist("Gone");
        await store.ReplacePlaylistList([kept, gone]);
        await store.SavePlaylistDetail(Detail(kept.ID));
        await store.SavePlaylistDetail(Detail(gone.ID));

        await store.ReplacePlaylistList([kept]);

        Assert.Equal([kept.ID], await store.GetHeldPlaylistIDList());
        Assert.NotNull(await store.GetPlaylistDetail(kept.ID));
        Assert.Null(await store.GetPlaylistDetail(gone.ID));
    }

    [Fact]
    public async Task SavePlaylist_LeavesAHeldDetailAlone()
    {
        IMetadataStore store = CreateStore();
        ResponsePlaylist playlist = Playlist();
        await store.ReplacePlaylistList([playlist]);
        ResponsePlaylistDetail detail = Detail(playlist.ID);
        await store.SavePlaylistDetail(detail);

        await store.SavePlaylist(Playlist("Renamed", playlist.ID));

        AssertSame(detail, await store.GetPlaylistDetail(playlist.ID));
    }

    #endregion -- Playlists --

    #region -- Playlist details --

    [Fact]
    public async Task SavePlaylistDetail_MakesThePlaylistHeld_AndReturnsEveryFieldIncludingRepeatedMediaFiles()
    {
        IMetadataStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        ResponsePlaylistDetail detail = Detail(id, FullMediaFile());

        await store.SavePlaylistDetail(detail);

        Assert.Equal([id], await store.GetHeldPlaylistIDList());
        ResponsePlaylistDetail? read = await store.GetPlaylistDetail(id);
        AssertSame(detail, read);
        Assert.Equal(3, read?.EntryList.Count);
        Assert.Equal(read?.EntryList[0].MediaFileID, read?.EntryList[1].MediaFileID);
    }

    [Fact]
    public async Task SavePlaylistDetail_AgainReplacesTheOldOne()
    {
        IMetadataStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await store.SavePlaylistDetail(Detail(id));
        ResponsePlaylistDetail newer = Detail(id);

        await store.SavePlaylistDetail(newer);

        AssertSame(newer, await store.GetPlaylistDetail(id));
        Assert.Single(await store.GetHeldPlaylistIDList());
    }

    #endregion -- Playlist details --

    #region -- ETags --

    [Fact]
    public async Task ETags_RoundTripIndependently_AndNullClearsOne()
    {
        IMetadataStore store = CreateStore();

        await store.SetLibraryETag("\"library-1\"");
        await store.SetPlaylistListETag("\"playlists-1\"");

        Assert.Equal("\"library-1\"", await store.GetLibraryETag());
        Assert.Equal("\"playlists-1\"", await store.GetPlaylistListETag());

        await store.SetLibraryETag(null);

        Assert.Null(await store.GetLibraryETag());
        Assert.Equal("\"playlists-1\"", await store.GetPlaylistListETag());
    }

    [Fact]
    public async Task SettingAnETagAgain_ReplacesIt()
    {
        IMetadataStore store = CreateStore();
        await store.SetLibraryETag("\"one\"");

        await store.SetLibraryETag("\"two\"");

        Assert.Equal("\"two\"", await store.GetLibraryETag());
    }

    #endregion -- ETags --

    #region -- File states --

    private static FileState OnDevice(Guid id, string hash = "abc")
    {
        return new(id, FileSyncState.OnDevice, hash, null);
    }

    [Fact]
    public async Task AnEmptyStore_HasNoFileStates()
    {
        IMetadataStore store = CreateStore();

        Assert.Empty(await store.GetFileStateList());
    }

    [Fact]
    public async Task SaveFileState_InsertsOrReplacesByMediaFileID_AndReturnsEveryField()
    {
        IMetadataStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        Guid other = Guid.CreateVersion7();

        await store.SaveFileState(new(id, FileSyncState.Failed, null, "The hash did not match."));
        await store.SaveFileState(OnDevice(other, "def"));

        List<FileState> list = await store.GetFileStateList();
        Assert.Equal(2, list.Count);
        Assert.Equal(new FileState(id, FileSyncState.Failed, null, "The hash did not match."), list.Single(x => x.MediaFileID == id));
        Assert.Equal(OnDevice(other, "def"), list.Single(x => x.MediaFileID == other));

        await store.SaveFileState(OnDevice(id, "ghi"));

        list = await store.GetFileStateList();
        Assert.Equal(2, list.Count);
        Assert.Equal(OnDevice(id, "ghi"), list.Single(x => x.MediaFileID == id));
    }

    [Fact]
    public async Task DeleteFileState_RemovesIt_AndAnUnknownIDIsHarmless()
    {
        IMetadataStore store = CreateStore();
        Guid kept = Guid.CreateVersion7();
        Guid gone = Guid.CreateVersion7();
        await store.SaveFileState(OnDevice(kept));
        await store.SaveFileState(OnDevice(gone));

        await store.DeleteFileState(gone);
        await store.DeleteFileState(Guid.CreateVersion7());

        Assert.Equal([kept], (await store.GetFileStateList()).Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task DeleteMediaFile_AlsoDeletesThatMediaFilesState()
    {
        IMetadataStore store = CreateStore();
        ResponseMediaFile gone = MediaFile("Gone");
        ResponseMediaFile kept = MediaFile("Kept");
        await store.ReplaceMediaFileList([gone, kept]);
        await store.SaveFileState(OnDevice(gone.ID));
        await store.SaveFileState(OnDevice(kept.ID));

        await store.DeleteMediaFile(gone.ID);

        Assert.Equal([kept.ID], (await store.GetFileStateList()).Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task ReplaceMediaFileList_RemovesTheStateOfMediaFilesNoLongerInTheList()
    {
        IMetadataStore store = CreateStore();
        ResponseMediaFile kept = MediaFile("Kept");
        ResponseMediaFile gone = MediaFile("Gone");
        await store.ReplaceMediaFileList([kept, gone]);
        await store.SaveFileState(OnDevice(kept.ID));
        await store.SaveFileState(OnDevice(gone.ID));

        await store.ReplaceMediaFileList([kept]);

        Assert.Equal([kept.ID], (await store.GetFileStateList()).Select(x => x.MediaFileID));
    }

    #endregion -- File states --
}
