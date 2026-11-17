using System.Net;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class MetadataSyncServiceTest
{
    private static readonly DateTime Start = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeApiClient _api = new();
    private readonly FakeMetadataStore _store = new();
    private readonly MetadataSyncService _service;

    public MetadataSyncServiceTest()
    {
        _service = new(_api, _store);
    }

    private static ResponseArtist MakeArtist(string label, Guid? id = null, DateTime? timeModified = null)
    {
        return new() { ID = id ?? Guid.CreateVersion7(), Name = label, MediaFileCount = 1, TimeModified = timeModified ?? Start };
    }

    private static ResponseAlbum MakeAlbum(string label, Guid? id = null, DateTime? timeModified = null)
    {
        return new() { ID = id ?? Guid.CreateVersion7(), Title = label, ArtistIDList = [Guid.CreateVersion7()], MediaFileCount = 1, TimeModified = timeModified ?? Start };
    }

    private static ResponseGenre MakeGenre(string label, Guid? id = null, DateTime? timeModified = null)
    {
        return new() { ID = id ?? Guid.CreateVersion7(), Name = label, MediaFileCount = 1, TimeModified = timeModified ?? Start };
    }

    private static string Label(ResponseArtist artist)
    {
        return artist.Name;
    }

    private static string Label(ResponseAlbum album)
    {
        return album.Title;
    }

    private static string Label(ResponseGenre genre)
    {
        return genre.Name;
    }

    #region -- Artist --

    [Fact]
    public async Task SyncArtistList_FirstRun_SendsNoETag_StoresTheListAndTheETag()
    {
        _api.ArtistListHandler = _ => new(HttpStatusCode.OK, [MakeArtist("One")], "\"e1\"");

        await _service.SyncArtistList();

        Assert.Equal([null], _api.ArtistListETagList);
        Assert.Equal(["One"], (await _store.Artist.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Artist.GetETag());
    }

    [Fact]
    public async Task SyncArtistList_On304_ChangesNothing()
    {
        await _store.Artist.ReplaceList([MakeArtist("One")]);
        await _store.Artist.SetETag("\"e1\"");
        _api.ArtistListHandler = _ => new(HttpStatusCode.NotModified, null, null);
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.SyncArtistList();

        Assert.Equal(["\"e1\""], _api.ArtistListETagList);
        Assert.Equal(["One"], (await _store.Artist.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Artist.GetETag());
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task SyncArtistList_On200_ReplacesTheLocalList_AndRaisesChanged()
    {
        await _store.Artist.ReplaceList([MakeArtist("Old")]);
        await _store.Artist.SetETag("\"old\"");
        _api.ArtistListHandler = _ => new(HttpStatusCode.OK, [MakeArtist("New1"), MakeArtist("New2")], "\"new\"");
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.SyncArtistList();

        Assert.Equal(["New1", "New2"], (await _store.Artist.GetList()).Select(Label));
        Assert.Equal("\"new\"", await _store.Artist.GetETag());
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task SyncArtistList_WhenTheLocalListIsEmptyButAnETagIsStored_SendsNoETag()
    {
        await _store.Artist.SetETag("\"stale\"");
        _api.ArtistListHandler = _ => new(HttpStatusCode.OK, [MakeArtist("One")], "\"fresh\"");

        await _service.SyncArtistList();

        Assert.Equal([null], _api.ArtistListETagList);
    }

    [Fact]
    public async Task SyncArtistList_Force_SendsNoETag()
    {
        await _store.Artist.ReplaceList([MakeArtist("One")]);
        await _store.Artist.SetETag("\"e1\"");
        _api.ArtistListHandler = _ => new(HttpStatusCode.OK, [MakeArtist("Two")], "\"e2\"");

        await _service.SyncArtistList(true);

        Assert.Equal([null], _api.ArtistListETagList);
        Assert.Equal(["Two"], (await _store.Artist.GetList()).Select(Label));
    }

    [Fact]
    public async Task SyncArtistList_WhenOffline_LeavesListAndETagAlone()
    {
        await _store.Artist.ReplaceList([MakeArtist("One")]);
        await _store.Artist.SetETag("\"e1\"");
        _api.ArtistListHandler = _ => throw new HttpRequestException("The network is down.");

        await _service.SyncArtistList();

        Assert.Equal(["One"], (await _store.Artist.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Artist.GetETag());
    }

    [Fact]
    public async Task ApplyArtistUpdated_SavesIt_AndKeepsTheStoredETag()
    {
        ResponseArtist existing = MakeArtist("Before");
        await _store.Artist.ReplaceList([existing]);
        await _store.Artist.SetETag("\"e1\"");
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.ApplyArtistUpdated(MakeArtist("After", existing.ID, Start.AddMinutes(1)));
        await _service.ApplyArtistUpdated(MakeArtist("Added"));

        Assert.Equal(["After", "Added"], (await _store.Artist.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Artist.GetETag());
        Assert.Equal(2, changed);
    }

    [Fact]
    public async Task ApplyArtistUpdated_WithAnOlderOrEqualTimeModified_IsIgnored()
    {
        ResponseArtist existing = MakeArtist("Current");
        await _store.Artist.ReplaceList([existing]);
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.ApplyArtistUpdated(MakeArtist("Older", existing.ID, Start.AddMinutes(-1)));
        await _service.ApplyArtistUpdated(MakeArtist("Equal", existing.ID, Start));

        Assert.Equal(["Current"], (await _store.Artist.GetList()).Select(Label));
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task ApplyArtistDeleted_RemovesIt_AndAnUnknownIDIsHarmless()
    {
        ResponseArtist existing = MakeArtist("One");
        await _store.Artist.ReplaceList([existing, MakeArtist("Two")]);
        await _store.Artist.SetETag("\"e1\"");

        await _service.ApplyArtistDeleted(existing.ID);
        await _service.ApplyArtistDeleted(Guid.CreateVersion7());

        Assert.Equal(["Two"], (await _store.Artist.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Artist.GetETag());
    }

    #endregion -- Artist --

    #region -- Album --

    [Fact]
    public async Task SyncAlbumList_FirstRun_SendsNoETag_StoresTheListAndTheETag()
    {
        _api.AlbumListHandler = _ => new(HttpStatusCode.OK, [MakeAlbum("One")], "\"e1\"");

        await _service.SyncAlbumList();

        Assert.Equal([null], _api.AlbumListETagList);
        Assert.Equal(["One"], (await _store.Album.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Album.GetETag());
    }

    [Fact]
    public async Task SyncAlbumList_On304_ChangesNothing()
    {
        await _store.Album.ReplaceList([MakeAlbum("One")]);
        await _store.Album.SetETag("\"e1\"");
        _api.AlbumListHandler = _ => new(HttpStatusCode.NotModified, null, null);
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.SyncAlbumList();

        Assert.Equal(["\"e1\""], _api.AlbumListETagList);
        Assert.Equal(["One"], (await _store.Album.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Album.GetETag());
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task SyncAlbumList_On200_ReplacesTheLocalList_AndRaisesChanged()
    {
        await _store.Album.ReplaceList([MakeAlbum("Old")]);
        await _store.Album.SetETag("\"old\"");
        _api.AlbumListHandler = _ => new(HttpStatusCode.OK, [MakeAlbum("New1"), MakeAlbum("New2")], "\"new\"");
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.SyncAlbumList();

        Assert.Equal(["New1", "New2"], (await _store.Album.GetList()).Select(Label));
        Assert.Equal("\"new\"", await _store.Album.GetETag());
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task SyncAlbumList_WhenTheLocalListIsEmptyButAnETagIsStored_SendsNoETag()
    {
        await _store.Album.SetETag("\"stale\"");
        _api.AlbumListHandler = _ => new(HttpStatusCode.OK, [MakeAlbum("One")], "\"fresh\"");

        await _service.SyncAlbumList();

        Assert.Equal([null], _api.AlbumListETagList);
    }

    [Fact]
    public async Task SyncAlbumList_Force_SendsNoETag()
    {
        await _store.Album.ReplaceList([MakeAlbum("One")]);
        await _store.Album.SetETag("\"e1\"");
        _api.AlbumListHandler = _ => new(HttpStatusCode.OK, [MakeAlbum("Two")], "\"e2\"");

        await _service.SyncAlbumList(true);

        Assert.Equal([null], _api.AlbumListETagList);
        Assert.Equal(["Two"], (await _store.Album.GetList()).Select(Label));
    }

    [Fact]
    public async Task SyncAlbumList_WhenOffline_LeavesListAndETagAlone()
    {
        await _store.Album.ReplaceList([MakeAlbum("One")]);
        await _store.Album.SetETag("\"e1\"");
        _api.AlbumListHandler = _ => throw new HttpRequestException("The network is down.");

        await _service.SyncAlbumList();

        Assert.Equal(["One"], (await _store.Album.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Album.GetETag());
    }

    [Fact]
    public async Task ApplyAlbumUpdated_SavesIt_AndKeepsTheStoredETag()
    {
        ResponseAlbum existing = MakeAlbum("Before");
        await _store.Album.ReplaceList([existing]);
        await _store.Album.SetETag("\"e1\"");
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.ApplyAlbumUpdated(MakeAlbum("After", existing.ID, Start.AddMinutes(1)));
        await _service.ApplyAlbumUpdated(MakeAlbum("Added"));

        Assert.Equal(["After", "Added"], (await _store.Album.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Album.GetETag());
        Assert.Equal(2, changed);
    }

    [Fact]
    public async Task ApplyAlbumUpdated_WithAnOlderOrEqualTimeModified_IsIgnored()
    {
        ResponseAlbum existing = MakeAlbum("Current");
        await _store.Album.ReplaceList([existing]);
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.ApplyAlbumUpdated(MakeAlbum("Older", existing.ID, Start.AddMinutes(-1)));
        await _service.ApplyAlbumUpdated(MakeAlbum("Equal", existing.ID, Start));

        Assert.Equal(["Current"], (await _store.Album.GetList()).Select(Label));
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task ApplyAlbumDeleted_RemovesIt_AndAnUnknownIDIsHarmless()
    {
        ResponseAlbum existing = MakeAlbum("One");
        await _store.Album.ReplaceList([existing, MakeAlbum("Two")]);
        await _store.Album.SetETag("\"e1\"");

        await _service.ApplyAlbumDeleted(existing.ID);
        await _service.ApplyAlbumDeleted(Guid.CreateVersion7());

        Assert.Equal(["Two"], (await _store.Album.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Album.GetETag());
    }

    #endregion -- Album --

    #region -- Genre --

    [Fact]
    public async Task SyncGenreList_FirstRun_SendsNoETag_StoresTheListAndTheETag()
    {
        _api.GenreListHandler = _ => new(HttpStatusCode.OK, [MakeGenre("One")], "\"e1\"");

        await _service.SyncGenreList();

        Assert.Equal([null], _api.GenreListETagList);
        Assert.Equal(["One"], (await _store.Genre.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Genre.GetETag());
    }

    [Fact]
    public async Task SyncGenreList_On304_ChangesNothing()
    {
        await _store.Genre.ReplaceList([MakeGenre("One")]);
        await _store.Genre.SetETag("\"e1\"");
        _api.GenreListHandler = _ => new(HttpStatusCode.NotModified, null, null);
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.SyncGenreList();

        Assert.Equal(["\"e1\""], _api.GenreListETagList);
        Assert.Equal(["One"], (await _store.Genre.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Genre.GetETag());
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task SyncGenreList_On200_ReplacesTheLocalList_AndRaisesChanged()
    {
        await _store.Genre.ReplaceList([MakeGenre("Old")]);
        await _store.Genre.SetETag("\"old\"");
        _api.GenreListHandler = _ => new(HttpStatusCode.OK, [MakeGenre("New1"), MakeGenre("New2")], "\"new\"");
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.SyncGenreList();

        Assert.Equal(["New1", "New2"], (await _store.Genre.GetList()).Select(Label));
        Assert.Equal("\"new\"", await _store.Genre.GetETag());
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task SyncGenreList_WhenTheLocalListIsEmptyButAnETagIsStored_SendsNoETag()
    {
        await _store.Genre.SetETag("\"stale\"");
        _api.GenreListHandler = _ => new(HttpStatusCode.OK, [MakeGenre("One")], "\"fresh\"");

        await _service.SyncGenreList();

        Assert.Equal([null], _api.GenreListETagList);
    }

    [Fact]
    public async Task SyncGenreList_Force_SendsNoETag()
    {
        await _store.Genre.ReplaceList([MakeGenre("One")]);
        await _store.Genre.SetETag("\"e1\"");
        _api.GenreListHandler = _ => new(HttpStatusCode.OK, [MakeGenre("Two")], "\"e2\"");

        await _service.SyncGenreList(true);

        Assert.Equal([null], _api.GenreListETagList);
        Assert.Equal(["Two"], (await _store.Genre.GetList()).Select(Label));
    }

    [Fact]
    public async Task SyncGenreList_WhenOffline_LeavesListAndETagAlone()
    {
        await _store.Genre.ReplaceList([MakeGenre("One")]);
        await _store.Genre.SetETag("\"e1\"");
        _api.GenreListHandler = _ => throw new HttpRequestException("The network is down.");

        await _service.SyncGenreList();

        Assert.Equal(["One"], (await _store.Genre.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Genre.GetETag());
    }

    [Fact]
    public async Task ApplyGenreUpdated_SavesIt_AndKeepsTheStoredETag()
    {
        ResponseGenre existing = MakeGenre("Before");
        await _store.Genre.ReplaceList([existing]);
        await _store.Genre.SetETag("\"e1\"");
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.ApplyGenreUpdated(MakeGenre("After", existing.ID, Start.AddMinutes(1)));
        await _service.ApplyGenreUpdated(MakeGenre("Added"));

        Assert.Equal(["After", "Added"], (await _store.Genre.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Genre.GetETag());
        Assert.Equal(2, changed);
    }

    [Fact]
    public async Task ApplyGenreUpdated_WithAnOlderOrEqualTimeModified_IsIgnored()
    {
        ResponseGenre existing = MakeGenre("Current");
        await _store.Genre.ReplaceList([existing]);
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.ApplyGenreUpdated(MakeGenre("Older", existing.ID, Start.AddMinutes(-1)));
        await _service.ApplyGenreUpdated(MakeGenre("Equal", existing.ID, Start));

        Assert.Equal(["Current"], (await _store.Genre.GetList()).Select(Label));
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task ApplyGenreDeleted_RemovesIt_AndAnUnknownIDIsHarmless()
    {
        ResponseGenre existing = MakeGenre("One");
        await _store.Genre.ReplaceList([existing, MakeGenre("Two")]);
        await _store.Genre.SetETag("\"e1\"");

        await _service.ApplyGenreDeleted(existing.ID);
        await _service.ApplyGenreDeleted(Guid.CreateVersion7());

        Assert.Equal(["Two"], (await _store.Genre.GetList()).Select(Label));
        Assert.Equal("\"e1\"", await _store.Genre.GetETag());
    }

    #endregion -- Genre --

    #region -- Independence and failures --

    [Fact]
    public async Task TheThreeKinds_NeverTouchEachOthersListsOrETags()
    {
        Guid sharedID = Guid.CreateVersion7();
        await _store.Album.ReplaceList([MakeAlbum("Album", sharedID)]);
        await _store.Genre.ReplaceList([MakeGenre("Genre", sharedID)]);
        await _store.Album.SetETag("\"b\"");
        await _store.Genre.SetETag("\"g\"");
        _api.ArtistListHandler = _ => new(HttpStatusCode.OK, [MakeArtist("Artist", sharedID)], "\"a\"");

        await _service.SyncArtistList();
        await _service.ApplyArtistDeleted(sharedID);

        Assert.Empty(await _store.Artist.GetList());
        Assert.Single(await _store.Album.GetList());
        Assert.Single(await _store.Genre.GetList());
        Assert.Equal("\"b\"", await _store.Album.GetETag());
        Assert.Equal("\"g\"", await _store.Genre.GetETag());
        Assert.Empty(_api.AlbumListETagList);
        Assert.Empty(_api.GenreListETagList);
    }

    [Fact]
    public async Task ARefusedSync_Throws_AndLeavesTheList()
    {
        await _store.Genre.ReplaceList([MakeGenre("One")]);
        _api.GenreListHandler = _ => throw new ApiException(HttpStatusCode.InternalServerError, "Broken.");

        await Assert.ThrowsAsync<ApiException>(() => _service.SyncGenreList());

        Assert.Equal(["One"], (await _store.Genre.GetList()).Select(Label));
    }

    #endregion -- Independence and failures --
}
