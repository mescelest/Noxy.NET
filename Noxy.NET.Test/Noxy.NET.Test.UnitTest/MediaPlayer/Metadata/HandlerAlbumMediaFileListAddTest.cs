using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerAlbumMediaFileListAddTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerAlbumMediaFileListAdd _handler;

    public HandlerAlbumMediaFileListAddTest()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);
        _handler = new(new FakeUnitOfWorkFactory(_store), new NameKeyService(), new MetadataChangeService(), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityAlbum SeedAlbum(string title, Guid? userID = null)
    {
        EntityAlbum album = new() { UserID = userID ?? UserID, Title = title, TitleKey = title.ToLowerInvariant(), IdentityKey = $"{title.ToLowerInvariant()}|", TimeModified = Original };
        _store.AlbumList.Add(album);
        return album;
    }

    private EntityMediaFile SeedFile(string title, EntityAlbum? album = null, Guid? userID = null)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new string('a', 64), UserID = userID ?? UserID, TimeModified = Original, AlbumID = album?.ID };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    [Fact]
    public async Task Add_SetsTheAlbumOfEachFile_ReplacingTheOneItHad()
    {
        EntityAlbum album = SeedAlbum("Target");
        EntityAlbum previous = SeedAlbum("Previous");
        EntityMediaFile fresh = SeedFile("One");
        EntityMediaFile replaced = SeedFile("Two", previous);

        ResponseAlbum response = await _handler.Handle(new CommandAlbumMediaFileListAdd(UserID, album.ID, [fresh.ID, replaced.ID]), Token);

        Assert.Equal(album.ID, Stored(fresh).AlbumID);
        Assert.Equal(album.ID, Stored(replaced).AlbumID);
        Assert.Equal(2, response.MediaFileCount);
        Assert.Equal(_time.Now.UtcDateTime, Stored(fresh).TimeModified);
        Assert.Equal(_time.Now.UtcDateTime, Stored(replaced).TimeModified);
        Assert.Equal(2, _publisher.GetList<NotificationMediaUpdated>().Count);
        Assert.All(_publisher.GetList<NotificationMediaUpdated>(), x => Assert.Equal("Target", x.MediaFile.Album));
        Assert.Equal(album.ID, Assert.Single(_publisher.GetList<NotificationAlbumUpdated>()).Album.ID);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Add_AFileAlreadyInTheAlbum_IsLeftAlone_AndWhenNoneChangesNothingIsSavedOrPushed()
    {
        EntityAlbum album = SeedAlbum("Target");
        EntityMediaFile file = SeedFile("One", album);

        ResponseAlbum response = await _handler.Handle(new CommandAlbumMediaFileListAdd(UserID, album.ID, [file.ID]), Token);

        Assert.Equal(1, response.MediaFileCount);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
        Assert.Equal(Original, Stored(file).TimeModified);
    }

    [Fact]
    public async Task Add_AnUnknownOrForeignFileIsNotFound_AndAForeignAlbumIsNotFound()
    {
        EntityAlbum album = SeedAlbum("Target");
        EntityAlbum foreignAlbum = SeedAlbum("Theirs", Guid.CreateVersion7());
        EntityMediaFile valid = SeedFile("One");
        EntityMediaFile foreignFile = SeedFile("Theirs", null, Guid.CreateVersion7());

        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new CommandAlbumMediaFileListAdd(UserID, album.ID, [valid.ID, foreignFile.ID]), Token).AsTask());
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _handler.Handle(new CommandAlbumMediaFileListAdd(UserID, foreignAlbum.ID, [valid.ID]), Token).AsTask());

        Assert.Null(Stored(valid).AlbumID);
        Assert.Equal(0, _store.CommitCount);
    }
}
