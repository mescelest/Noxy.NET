using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
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

    private EntityAlbum Seed(Guid? userID = null, DateTime? timeArtModified = null, string identityKey = "a|")
    {
        EntityAlbum album = new() { UserID = userID ?? UserID, Title = "A", TitleKey = "a", IdentityKey = identityKey, TimeArtModified = timeArtModified };
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
    public async Task Inherit_CopiesThePicture_SetsTheTime_AndPublishesAlbumUpdated()
    {
        EntityAlbum from = Seed(timeArtModified: _time.Now.UtcDateTime.AddDays(-1));
        _storage.AlbumArtList[from.ID] = [7, 7];
        EntityAlbum to = Seed(identityKey: "b|");

        bool copied = await _service.Inherit(UserID, from.ID, to.ID, Token);

        Assert.True(copied);
        Assert.Equal([7, 7], _storage.AlbumArtList[to.ID]);
        Assert.Equal([7, 7], _storage.AlbumArtList[from.ID]);
        Assert.Equal(_time.Now.UtcDateTime, to.TimeArtModified);
        NotificationAlbumUpdated notification = Assert.Single(_publisher.GetList<NotificationAlbumUpdated>());
        Assert.Equal(to.ID, notification.Album.ID);
    }

    [Fact]
    public async Task Inherit_IntoAnAlbumThatHasAPicture_ChangesNothing()
    {
        EntityAlbum from = Seed(timeArtModified: _time.Now.UtcDateTime.AddDays(-2));
        _storage.AlbumArtList[from.ID] = [7];
        EntityAlbum to = Seed(identityKey: "b|", timeArtModified: _time.Now.UtcDateTime.AddDays(-1));
        _storage.AlbumArtList[to.ID] = [8];

        bool copied = await _service.Inherit(UserID, from.ID, to.ID, Token);

        Assert.False(copied);
        Assert.Equal([8], _storage.AlbumArtList[to.ID]);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Inherit_FromAnAlbumWithNoPicture_OrAnotherUsersAlbum_ChangesNothing()
    {
        EntityAlbum bare = Seed(identityKey: "c|");
        EntityAlbum foreign = Seed(Guid.CreateVersion7(), _time.Now.UtcDateTime.AddDays(-1), "d|");
        _storage.AlbumArtList[foreign.ID] = [7];
        EntityAlbum to = Seed(identityKey: "b|");

        Assert.False(await _service.Inherit(UserID, bare.ID, to.ID, Token));
        Assert.False(await _service.Inherit(UserID, foreign.ID, to.ID, Token));
        Assert.Null(to.TimeArtModified);
        Assert.DoesNotContain(to.ID, _storage.AlbumArtList.Keys);
    }

    private EntityMediaFile FileOf(EntityAlbum album)
    {
        return _store.MediaFileList.Single(x => x.AlbumID == album.ID);
    }

    [Fact]
    public async Task Apply_TouchesAndPushesTheFilesOfTheAlbum()
    {
        EntityAlbum album = Seed();
        DateTime before = FileOf(album).TimeModified;

        await _service.Apply(UserID, album.ID, Picture, Token);

        Assert.NotEqual(before, FileOf(album).TimeModified);
        Assert.Equal(_time.Now.UtcDateTime, FileOf(album).TimeModified);
        NotificationMediaUpdated pushed = Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal(FileOf(album).ID, pushed.MediaFile.ID);
        Assert.Equal(UserID, pushed.UserID);
    }

    [Fact]
    public async Task Replace_TouchesAndPushesTheFilesOfTheAlbum()
    {
        EntityAlbum album = Seed(timeArtModified: _time.Now.UtcDateTime.AddDays(-1));

        await _service.Replace(UserID, album.ID, Picture, Token);

        Assert.Equal(_time.Now.UtcDateTime, FileOf(album).TimeModified);
        Assert.Equal(FileOf(album).ID, Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.ID);
    }

    [Fact]
    public async Task Inherit_TouchesAndPushesTheFilesOfTheTargetAlbumOnly()
    {
        EntityAlbum from = Seed(timeArtModified: _time.Now.UtcDateTime.AddDays(-1));
        _storage.AlbumArtList[from.ID] = [7];
        EntityAlbum to = Seed(identityKey: "b|");
        DateTime fromBefore = FileOf(from).TimeModified;

        await _service.Inherit(UserID, from.ID, to.ID, Token);

        Assert.Equal(_time.Now.UtcDateTime, FileOf(to).TimeModified);
        Assert.Equal(fromBefore, FileOf(from).TimeModified);
        Assert.Equal(FileOf(to).ID, Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.ID);
    }

    [Fact]
    public async Task Apply_ToAnAlbumThatKeepsItsPicture_TouchesNothing()
    {
        EntityAlbum album = Seed(timeArtModified: _time.Now.UtcDateTime.AddDays(-1));
        DateTime before = FileOf(album).TimeModified;

        await _service.Apply(UserID, album.ID, Picture, Token);

        Assert.Equal(before, FileOf(album).TimeModified);
        Assert.Empty(_publisher.GetList<NotificationMediaUpdated>());
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

    [Fact]
    public async Task Replace_ForAnAlbumThatHasAPicture_StoresTheNewOne_AndMovesTheTimes()
    {
        DateTime had = _time.Now.UtcDateTime.AddDays(-1);
        EntityAlbum album = Seed(timeArtModified: had);

        await _service.Replace(UserID, album.ID, Picture, Token);

        Assert.Equal(_thumbnail.Result, _storage.AlbumArtList[album.ID]);
        Assert.Equal(_time.Now.UtcDateTime, album.TimeArtModified);
        Assert.Equal(_time.Now.UtcDateTime, album.TimeModified);
        NotificationAlbumUpdated notification = Assert.Single(_publisher.GetList<NotificationAlbumUpdated>());
        Assert.Equal(_time.Now.UtcDateTime, notification.Album.TimeArtModified);
    }

    [Fact]
    public async Task Replace_ForAnAlbumWithNoPicture_StoresIt()
    {
        EntityAlbum album = Seed();

        await _service.Replace(UserID, album.ID, Picture, Token);

        Assert.NotNull(album.TimeArtModified);
        Assert.Single(_storage.AlbumArtList);
    }

    [Fact]
    public async Task Replace_WithBytesThatAreNotAnImage_ThrowsAndChangesNothing()
    {
        DateTime had = _time.Now.UtcDateTime.AddDays(-1);
        EntityAlbum album = Seed(timeArtModified: had);
        _thumbnail.Result = null;

        await Assert.ThrowsAsync<InvalidMetadataOperationException>(() => _service.Replace(UserID, album.ID, Picture, Token));

        Assert.Equal(had, album.TimeArtModified);
        Assert.Empty(_storage.AlbumArtList);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Replace_ForAnotherUsersAlbum_ThrowsNotFound_AndChangesNothing()
    {
        EntityAlbum album = Seed(Guid.CreateVersion7());

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _service.Replace(UserID, album.ID, Picture, Token));

        Assert.Null(album.TimeArtModified);
        Assert.Empty(_storage.AlbumArtList);
    }

    [Fact]
    public async Task Replace_ForAnUnknownAlbum_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _service.Replace(UserID, Guid.CreateVersion7(), Picture, Token));
    }
}
