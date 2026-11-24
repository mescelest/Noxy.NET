using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Art;

public class AlbumArtServiceTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly byte[] Picture = [1, 2, 3];

    private readonly FakeStore _store = new();
    private readonly FakeThumbnailService _thumbnail = new();
    private readonly FakeArtStorageService _storage = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly AlbumArtService _service;

    public AlbumArtServiceTest()
    {
        _service = new(new FakeUnitOfWorkFactory(_store), _thumbnail, _storage, _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityAlbum Seed(Guid? userID = null, DateTime? timeArtModified = null)
    {
        EntityAlbum album = new() { UserID = userID ?? UserID, Title = "A", TitleKey = "a", IdentityKey = "a|", TimeArtModified = timeArtModified };
        _store.AlbumList.Add(album);
        _store.MediaFileList.Add(new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new('a', 64), UserID = album.UserID, AlbumID = album.ID });
        return album;
    }

    [Fact]
    public async Task Apply_StoresAThumbnail_SetsTheTime_AndPublishesAlbumUpdated()
    {
        EntityAlbum album = Seed();

        bool stored = await _service.Apply(UserID, album.ID, Picture, Token);

        Assert.True(stored);
        Assert.Equal(_thumbnail.Result, _storage.AlbumArtList[album.ID]);
        Assert.Equal(_time.Now.UtcDateTime, album.TimeArtModified);
        NotificationAlbumUpdated notification = Assert.Single(_publisher.GetList<NotificationAlbumUpdated>());
        Assert.Equal(UserID, notification.UserID);
        Assert.Equal(album.ID, notification.Album.ID);
        Assert.Equal(_time.Now.UtcDateTime, notification.Album.TimeArtModified);
        Assert.Equal(1, notification.Album.MediaFileCount);
    }

    [Fact]
    public async Task Apply_ToAnAlbumThatHasAPicture_ChangesNothing()
    {
        DateTime had = _time.Now.UtcDateTime.AddDays(-1);
        EntityAlbum album = Seed(timeArtModified: had);

        bool stored = await _service.Apply(UserID, album.ID, Picture, Token);

        Assert.False(stored);
        Assert.Equal(had, album.TimeArtModified);
        Assert.Empty(_storage.AlbumArtList);
        Assert.Empty(_publisher.PublishedList);
        Assert.Empty(_thumbnail.ImageList);
    }

    [Fact]
    public async Task Apply_WithBytesThatAreNotAnImage_ChangesNothing_AndDoesNotThrow()
    {
        EntityAlbum album = Seed();
        _thumbnail.Result = null;

        bool stored = await _service.Apply(UserID, album.ID, Picture, Token);

        Assert.False(stored);
        Assert.Null(album.TimeArtModified);
        Assert.Empty(_storage.AlbumArtList);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Apply_ForAnotherUsersAlbum_ChangesNothing()
    {
        EntityAlbum album = Seed(Guid.CreateVersion7());

        bool stored = await _service.Apply(UserID, album.ID, Picture, Token);

        Assert.False(stored);
        Assert.Null(album.TimeArtModified);
        Assert.Empty(_storage.AlbumArtList);
    }

    [Fact]
    public async Task Apply_ForAnUnknownAlbum_ChangesNothing()
    {
        Assert.False(await _service.Apply(UserID, Guid.CreateVersion7(), Picture, Token));

        Assert.Empty(_storage.AlbumArtList);
    }

    [Fact]
    public async Task Apply_TwiceAtOnce_KeepsOnePicture_AndPublishesOnce()
    {
        EntityAlbum album = Seed();

        bool[] resultList = await Task.WhenAll(_service.Apply(UserID, album.ID, Picture, Token), _service.Apply(UserID, album.ID, [4, 5], Token));

        Assert.Single(resultList, true);
        Assert.Single(_storage.AlbumArtList);
        Assert.Single(_publisher.GetList<NotificationAlbumUpdated>());
    }

    [Fact]
    public async Task Apply_MovesTheModifiedTime_SoADeviceThatHoldsTheAlbumTakesThePush()
    {
        EntityAlbum album = Seed();
        DateTime before = album.TimeModified;
        _time.Now = new(before.AddDays(1), TimeSpan.Zero);

        await _service.Apply(UserID, album.ID, Picture, Token);

        Assert.Equal(before.AddDays(1), album.TimeModified);
        NotificationAlbumUpdated notification = Assert.Single(_publisher.GetList<NotificationAlbumUpdated>());
        Assert.Equal(before.AddDays(1), notification.Album.TimeModified);
    }
}
