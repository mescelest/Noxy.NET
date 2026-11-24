using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Art;

public class HandlerAlbumArtTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime ArtTime = new(2026, 11, 24, 10, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakeArtStorageService _storage = new();
    private readonly HandlerAlbumArt _handler;

    public HandlerAlbumArtTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new ETagService(), _storage);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityAlbum Seed(Guid? userID = null, DateTime? timeArtModified = null, byte[]? picture = null)
    {
        EntityAlbum album = new() { UserID = userID ?? UserID, Title = "A", TitleKey = "a", IdentityKey = "a|", TimeArtModified = timeArtModified };
        _store.AlbumList.Add(album);
        if (picture is not null) _storage.AlbumArtList[album.ID] = picture;
        return album;
    }

    private Task<AlbumArtPicture?> Get(Guid albumID, string? ifNoneMatch = null, Guid? userID = null)
    {
        return _handler.Handle(new(userID ?? UserID, albumID, ifNoneMatch), Token).AsTask();
    }

    [Fact]
    public async Task Handle_ReturnsTheBytesAndAQuotedETag()
    {
        EntityAlbum album = Seed(timeArtModified: ArtTime, picture: [1, 2, 3]);

        AlbumArtPicture? picture = await Get(album.ID);

        Assert.NotNull(picture);
        Assert.Equal<byte[]?>([1, 2, 3], picture.Bytes);
        Assert.Matches("^\"[0-9a-f]{64}\"$", picture.ETag);
    }

    [Fact]
    public async Task Handle_AMatchingIfNoneMatch_ReturnsTheETagAndNoBytes()
    {
        EntityAlbum album = Seed(timeArtModified: ArtTime, picture: [1, 2, 3]);
        AlbumArtPicture? first = await Get(album.ID);
        Assert.NotNull(first);

        AlbumArtPicture? second = await Get(album.ID, first.ETag);

        Assert.NotNull(second);
        Assert.Equal(first.ETag, second.ETag);
        Assert.Null(second.Bytes);
    }

    [Fact]
    public async Task Handle_TheETagFollowsTheArtTime_AndAStaleOneGetsTheBytes()
    {
        EntityAlbum album = Seed(timeArtModified: ArtTime, picture: [1, 2, 3]);
        AlbumArtPicture? before = await Get(album.ID);
        Assert.NotNull(before);

        album.TimeArtModified = ArtTime.AddMinutes(1);
        AlbumArtPicture? after = await Get(album.ID, before.ETag);

        Assert.NotNull(after);
        Assert.NotEqual(before.ETag, after.ETag);
        Assert.NotNull(after.Bytes);
    }

    [Fact]
    public async Task Handle_AnAlbumWithNoPicture_ReturnsNull()
    {
        EntityAlbum album = Seed();

        Assert.Null(await Get(album.ID));
    }

    [Fact]
    public async Task Handle_AnUnknownAlbum_ReturnsNull()
    {
        Assert.Null(await Get(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task Handle_AnotherUsersAlbum_ReturnsNull()
    {
        EntityAlbum album = Seed(Guid.CreateVersion7(), ArtTime, [1]);

        Assert.Null(await Get(album.ID));
    }

    [Fact]
    public async Task Handle_WhenTheFileIsGone_ReturnsNull()
    {
        EntityAlbum album = Seed(timeArtModified: ArtTime);

        Assert.Null(await Get(album.ID));
    }
}
