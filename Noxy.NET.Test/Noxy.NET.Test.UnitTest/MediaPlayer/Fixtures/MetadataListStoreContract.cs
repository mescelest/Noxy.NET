using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public abstract class MetadataListStoreContract
{
    private static readonly DateTime Start = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    protected abstract IMetadataListStore CreateStore();

    private static ResponseArtist Artist(string name = "Artist", Guid? id = null, int count = 3)
    {
        return new() { ID = id ?? Guid.CreateVersion7(), Name = name, MediaFileCount = count, TimeModified = Start };
    }

    private static ResponseAlbum Album(string title = "Album", Guid? id = null, int count = 3)
    {
        return new() { ID = id ?? Guid.CreateVersion7(), Title = title, ArtistIDList = [Guid.CreateVersion7(), Guid.CreateVersion7()], MediaFileCount = count, TimeModified = Start };
    }

    private static ResponseGenre Genre(string name = "Genre", Guid? id = null, int count = 3)
    {
        return new() { ID = id ?? Guid.CreateVersion7(), Name = name, MediaFileCount = count, TimeModified = Start };
    }

    #region -- Artists --

    [Fact]
    public async Task Artist_AnEmptyStore_HasNoneAndNoETag()
    {
        IMetadataListStore store = CreateStore();

        Assert.Empty(await store.GetArtistList());
        Assert.Null(await store.GetArtistListETag());
    }

    [Fact]
    public async Task Artist_Replace_ReplacesRatherThanMerges_AndKeepsTheOrder()
    {
        IMetadataListStore store = CreateStore();
        await store.ReplaceArtistList([Artist("Old")]);

        await store.ReplaceArtistList([Artist("B"), Artist("A"), Artist("C")]);

        Assert.Equal(["B", "A", "C"], (await store.GetArtistList()).Select(x => x.Name));
    }

    [Fact]
    public async Task Artist_Save_InsertsAtTheEnd_AndReplacesInPlace()
    {
        IMetadataListStore store = CreateStore();
        ResponseArtist first = Artist("First");
        await store.ReplaceArtistList([first, Artist("Second")]);

        await store.SaveArtist(Artist("Third"));
        await store.SaveArtist(Artist("Renamed", first.ID, 0));

        List<ResponseArtist> list = await store.GetArtistList();
        Assert.Equal(["Renamed", "Second", "Third"], list.Select(x => x.Name));
        Assert.Equal(0, list[0].MediaFileCount);
    }

    [Fact]
    public async Task Artist_Delete_RemovesIt_AndAnUnknownIDIsHarmless()
    {
        IMetadataListStore store = CreateStore();
        ResponseArtist artist = Artist();
        await store.ReplaceArtistList([artist, Artist("Other")]);

        await store.DeleteArtist(artist.ID);
        await store.DeleteArtist(Guid.CreateVersion7());

        Assert.Equal(["Other"], (await store.GetArtistList()).Select(x => x.Name));
    }

    [Fact]
    public async Task Artist_EveryFieldComesBackEqual()
    {
        IMetadataListStore store = CreateStore();
        ResponseArtist artist = new() { ID = Guid.CreateVersion7(), Name = "Ünïcödé \"Artist\"", MediaFileCount = 0, TimeModified = Start.AddMinutes(7) };

        await store.SaveArtist(artist);

        ResponseArtist stored = Assert.Single(await store.GetArtistList());
        Assert.Equal((artist.ID, artist.Name, artist.MediaFileCount, artist.TimeModified), (stored.ID, stored.Name, stored.MediaFileCount, stored.TimeModified));
    }

    [Fact]
    public async Task Artist_TheETag_RoundTrips_AndNullClearsIt()
    {
        IMetadataListStore store = CreateStore();

        await store.SetArtistListETag("\"a\"");
        Assert.Equal("\"a\"", await store.GetArtistListETag());

        await store.SetArtistListETag(null);
        Assert.Null(await store.GetArtistListETag());
    }

    #endregion -- Artists --

    #region -- Albums --

    [Fact]
    public async Task Album_AnEmptyStore_HasNoneAndNoETag()
    {
        IMetadataListStore store = CreateStore();

        Assert.Empty(await store.GetAlbumList());
        Assert.Null(await store.GetAlbumListETag());
    }

    [Fact]
    public async Task Album_Replace_ReplacesRatherThanMerges_AndKeepsTheOrder()
    {
        IMetadataListStore store = CreateStore();
        await store.ReplaceAlbumList([Album("Old")]);

        await store.ReplaceAlbumList([Album("B"), Album("A")]);

        Assert.Equal(["B", "A"], (await store.GetAlbumList()).Select(x => x.Title));
    }

    [Fact]
    public async Task Album_Save_InsertsAtTheEnd_AndReplacesInPlace()
    {
        IMetadataListStore store = CreateStore();
        ResponseAlbum first = Album("First");
        await store.ReplaceAlbumList([first, Album("Second")]);

        await store.SaveAlbum(Album("Third"));
        await store.SaveAlbum(Album("Renamed", first.ID));

        Assert.Equal(["Renamed", "Second", "Third"], (await store.GetAlbumList()).Select(x => x.Title));
    }

    [Fact]
    public async Task Album_Delete_RemovesIt_AndAnUnknownIDIsHarmless()
    {
        IMetadataListStore store = CreateStore();
        ResponseAlbum album = Album();
        await store.ReplaceAlbumList([album, Album("Other")]);

        await store.DeleteAlbum(album.ID);
        await store.DeleteAlbum(Guid.CreateVersion7());

        Assert.Equal(["Other"], (await store.GetAlbumList()).Select(x => x.Title));
    }

    [Fact]
    public async Task Album_EveryFieldComesBackEqual_IncludingTwoArtistIDs()
    {
        IMetadataListStore store = CreateStore();
        ResponseAlbum album = Album("Ünïcödé", count: 0);

        await store.SaveAlbum(album);

        ResponseAlbum stored = Assert.Single(await store.GetAlbumList());
        Assert.Equal(album.ID, stored.ID);
        Assert.Equal(album.Title, stored.Title);
        Assert.Equal(album.ArtistIDList, stored.ArtistIDList);
        Assert.Equal(0, stored.MediaFileCount);
        Assert.Equal(album.TimeModified, stored.TimeModified);
    }

    [Fact]
    public async Task Album_TheETag_RoundTrips_AndNullClearsIt()
    {
        IMetadataListStore store = CreateStore();

        await store.SetAlbumListETag("\"b\"");
        Assert.Equal("\"b\"", await store.GetAlbumListETag());

        await store.SetAlbumListETag(null);
        Assert.Null(await store.GetAlbumListETag());
    }

    #endregion -- Albums --

    #region -- Genres --

    [Fact]
    public async Task Genre_AnEmptyStore_HasNoneAndNoETag()
    {
        IMetadataListStore store = CreateStore();

        Assert.Empty(await store.GetGenreList());
        Assert.Null(await store.GetGenreListETag());
    }

    [Fact]
    public async Task Genre_Replace_ReplacesRatherThanMerges_AndKeepsTheOrder()
    {
        IMetadataListStore store = CreateStore();
        await store.ReplaceGenreList([Genre("Old")]);

        await store.ReplaceGenreList([Genre("B"), Genre("A")]);

        Assert.Equal(["B", "A"], (await store.GetGenreList()).Select(x => x.Name));
    }

    [Fact]
    public async Task Genre_Save_InsertsAtTheEnd_AndReplacesInPlace()
    {
        IMetadataListStore store = CreateStore();
        ResponseGenre first = Genre("First");
        await store.ReplaceGenreList([first, Genre("Second")]);

        await store.SaveGenre(Genre("Third"));
        await store.SaveGenre(Genre("Renamed", first.ID));

        Assert.Equal(["Renamed", "Second", "Third"], (await store.GetGenreList()).Select(x => x.Name));
    }

    [Fact]
    public async Task Genre_Delete_RemovesIt_AndAnUnknownIDIsHarmless()
    {
        IMetadataListStore store = CreateStore();
        ResponseGenre genre = Genre();
        await store.ReplaceGenreList([genre, Genre("Other")]);

        await store.DeleteGenre(genre.ID);
        await store.DeleteGenre(Guid.CreateVersion7());

        Assert.Equal(["Other"], (await store.GetGenreList()).Select(x => x.Name));
    }

    [Fact]
    public async Task Genre_TheETag_RoundTrips_AndNullClearsIt()
    {
        IMetadataListStore store = CreateStore();

        await store.SetGenreListETag("\"g\"");
        Assert.Equal("\"g\"", await store.GetGenreListETag());

        await store.SetGenreListETag(null);
        Assert.Null(await store.GetGenreListETag());
    }

    #endregion -- Genres --

    #region -- Independence --

    [Fact]
    public async Task TheThreeKinds_NeverTouchEachOthersListsOrETags()
    {
        IMetadataListStore store = CreateStore();
        Guid sharedID = Guid.CreateVersion7();
        await store.ReplaceArtistList([Artist("Artist", sharedID)]);
        await store.ReplaceAlbumList([Album("Album", sharedID)]);
        await store.ReplaceGenreList([Genre("Genre", sharedID)]);
        await store.SetArtistListETag("\"a\"");
        await store.SetAlbumListETag("\"b\"");
        await store.SetGenreListETag("\"g\"");

        await store.DeleteArtist(sharedID);
        await store.SetAlbumListETag(null);

        Assert.Empty(await store.GetArtistList());
        Assert.Single(await store.GetAlbumList());
        Assert.Single(await store.GetGenreList());
        Assert.Equal("\"a\"", await store.GetArtistListETag());
        Assert.Null(await store.GetAlbumListETag());
        Assert.Equal("\"g\"", await store.GetGenreListETag());
    }

    #endregion -- Independence --
}
