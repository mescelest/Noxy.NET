using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerAlbumMediaFileListRemoveTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerAlbumMediaFileListRemove _handler;

    public HandlerAlbumMediaFileListRemoveTest()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);
        _handler = new(new FakeUnitOfWorkFactory(_store), new NameKeyService(), new MetadataChangeService(), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityAlbum SeedAlbum(string title)
    {
        EntityAlbum album = new() { UserID = UserID, Title = title, TitleKey = title.ToLowerInvariant(), IdentityKey = $"{title.ToLowerInvariant()}|", TimeModified = Original };
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
    public async Task Remove_ClearsTheAlbumOnlyOfTheGivenFilesThatAreInIt()
    {
        EntityAlbum album = SeedAlbum("Record");
        EntityAlbum other = SeedAlbum("Other");
        EntityMediaFile inAlbum = SeedFile("One", album);
        EntityMediaFile alsoInAlbum = SeedFile("Two", album);
        EntityMediaFile inOther = SeedFile("Three", other);

        ResponseAlbum response = await _handler.Handle(new CommandAlbumMediaFileListRemove(UserID, album.ID, [inAlbum.ID, inOther.ID]), Token);

        Assert.Null(Stored(inAlbum).AlbumID);
        Assert.Equal(album.ID, Stored(alsoInAlbum).AlbumID);
        Assert.Equal(other.ID, Stored(inOther).AlbumID);
        Assert.Equal(1, response.MediaFileCount);
        Assert.Equal(_time.Now.UtcDateTime, Stored(inAlbum).TimeModified);
        Assert.Equal(Original, Stored(inOther).TimeModified);
        Assert.Null(Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.Album);
    }

    [Fact]
    public async Task Remove_NothingToClear_ChangesNothing_AndAForeignFileIsNotFound()
    {
        EntityAlbum album = SeedAlbum("Record");
        EntityMediaFile file = SeedFile("One");
        EntityMediaFile foreign = SeedFile("Theirs", album, Guid.CreateVersion7());

        await _handler.Handle(new CommandAlbumMediaFileListRemove(UserID, album.ID, [file.ID]), Token);
        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new CommandAlbumMediaFileListRemove(UserID, album.ID, [foreign.ID]), Token).AsTask());

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Remove_AnotherUsersAlbum_IsNotFound()
    {
        EntityAlbum foreign = new() { UserID = Guid.CreateVersion7(), Title = "Theirs", TitleKey = "theirs", IdentityKey = "theirs|" };
        _store.AlbumList.Add(foreign);
        EntityMediaFile file = SeedFile("One");

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _handler.Handle(new CommandAlbumMediaFileListRemove(UserID, foreign.ID, [file.ID]), Token).AsTask());
    }
}
