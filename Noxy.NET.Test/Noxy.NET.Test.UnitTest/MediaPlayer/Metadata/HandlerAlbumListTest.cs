using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerAlbumListTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly HandlerAlbumList _handler;
    private DateTime _now = Start;

    public HandlerAlbumListTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new ETagService());
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityAlbum Seed(string title, Guid? userID = null, int fileCount = 0, params Guid[] artistIDList)
    {
        _now = _now.AddSeconds(1);
        EntityAlbum album = new() { UserID = userID ?? UserID, Title = title, TitleKey = title.ToLowerInvariant(), IdentityKey = EntityAlbum.BuildIdentityKey(title.ToLowerInvariant(), artistIDList), TimeCreated = _now, TimeModified = _now };
        album.ArtistLinkList = [.. artistIDList.Select((x, index) => new EntityAlbumArtist { AlbumID = album.ID, ArtistID = x, Position = index })];
        _store.AlbumList.Add(album);

        for (int index = 0; index < fileCount; index++)
        {
            LinkNewFile(album, userID ?? UserID);
        }

        return album;
    }

    private void LinkNewFile(EntityAlbum album, Guid userID)
    {
        _store.MediaFileList.Add(new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new('a', 64), UserID = userID, AlbumID = album.ID });
    }

    private Task<MetadataLibrary<ResponseAlbum>> List(string? ifNoneMatch = null)
    {
        return _handler.Handle(new(UserID, ifNoneMatch), Token).AsTask();
    }

    [Fact]
    public async Task Handle_ReturnsTheOwnersAlbumsNewestFirstWithCountsAndArtistIDs()
    {
        Guid first = Guid.CreateVersion7();
        Guid second = Guid.CreateVersion7();
        Seed("Older", fileCount: 3, artistIDList: [first, second]);
        Seed("Newer");
        Seed("Foreign", Guid.CreateVersion7(), 5);

        MetadataLibrary<ResponseAlbum> library = await List();

        Assert.NotNull(library.ItemList);
        Assert.Equal(["Newer", "Older"], library.ItemList.Select(x => x.Title).ToList());
        Assert.Equal([0, 3], library.ItemList.Select(x => x.MediaFileCount).ToList());
        Assert.Empty(library.ItemList[0].ArtistIDList);
        Assert.Equal([first, second], library.ItemList[1].ArtistIDList);
    }

    [Fact]
    public async Task Handle_ARenameOrACountChange_ChangesTheETag_AndAnotherUsersAlbumDoesNot()
    {
        EntityAlbum album = Seed("A");
        string before = (await List()).ETag;

        Seed("Foreign", Guid.CreateVersion7(), 2);
        Assert.Equal(before, (await List()).ETag);

        LinkNewFile(album, UserID);
        string afterCount = (await List()).ETag;
        Assert.NotEqual(before, afterCount);

        album.TimeModified = album.TimeModified.AddSeconds(5);
        Assert.NotEqual(afterCount, (await List()).ETag);
    }

    [Fact]
    public async Task Handle_AMatchingIfNoneMatch_ReturnsNoList()
    {
        Seed("A");
        string eTag = (await List()).ETag;

        MetadataLibrary<ResponseAlbum> library = await List(eTag);

        Assert.Equal(eTag, library.ETag);
        Assert.Null(library.ItemList);
    }

    [Fact]
    public async Task Handle_NoAlbums_IsAnEmptyList()
    {
        MetadataLibrary<ResponseAlbum> library = await List();

        Assert.NotNull(library.ItemList);
        Assert.Empty(library.ItemList);
    }

    [Fact]
    public async Task Handle_WhenOnlyTheArtTimeChanges_TheETagChanges_AndTheResponseCarriesIt()
    {
        EntityAlbum album = Seed("A");
        string before = (await List()).ETag;

        album.TimeArtModified = Start.AddDays(1);
        MetadataLibrary<ResponseAlbum> library = await List();

        Assert.NotEqual(before, library.ETag);
        Assert.NotNull(library.ItemList);
        Assert.Equal(Start.AddDays(1), library.ItemList.Single().TimeArtModified);

        string afterFirst = library.ETag;
        album.TimeArtModified = Start.AddDays(2);
        Assert.NotEqual(afterFirst, (await List()).ETag);
    }

    [Fact]
    public async Task Handle_AnAlbumWithNoPicture_HasNoArtTime()
    {
        Seed("A");

        MetadataLibrary<ResponseAlbum> library = await List();

        Assert.NotNull(library.ItemList);
        Assert.Null(library.ItemList.Single().TimeArtModified);
    }

    [Fact]
    public async Task Handle_AMatchingIfNoneMatch_IsStaleAfterTheArtTimeChanges()
    {
        EntityAlbum album = Seed("A");
        string eTag = (await List()).ETag;

        album.TimeArtModified = Start.AddDays(1);
        MetadataLibrary<ResponseAlbum> library = await List(eTag);

        Assert.NotNull(library.ItemList);
    }
}
