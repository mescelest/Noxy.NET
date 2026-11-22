using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerAlbumDeleteTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerAlbumDelete _handler;

    public HandlerAlbumDeleteTest()
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

    private EntityMediaFile SeedFile(string title, EntityAlbum? album)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new('a', 64), UserID = UserID, TimeModified = Original, AlbumID = album?.ID };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        _store.MediaFileList.Add(file);
        return file;
    }

    [Fact]
    public async Task Delete_RemovesTheAlbumFromItsFilesWhichStayAndArePushed()
    {
        EntityAlbum album = SeedAlbum("Record");
        EntityAlbum other = SeedAlbum("Other");
        EntityMediaFile had = SeedFile("One", album);
        EntityMediaFile unrelated = SeedFile("Two", other);

        await _handler.Handle(new(UserID, album.ID), Token);

        Assert.Equal(other.ID, Assert.Single(_store.AlbumList).ID);
        Assert.Equal(2, _store.MediaFileList.Count);
        EntityMediaFile stored = _store.MediaFileList.Single(x => x.ID == had.ID);
        Assert.Null(stored.AlbumID);
        Assert.Equal(_time.Now.UtcDateTime, stored.TimeModified);
        Assert.Equal(Original, _store.MediaFileList.Single(x => x.ID == unrelated.ID).TimeModified);
        NotificationMediaUpdated pushed = Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Null(pushed.MediaFile.AlbumID);
        Assert.Null(pushed.MediaFile.Album);
        Assert.Equal(album.ID, Assert.Single(_publisher.GetList<NotificationAlbumDeleted>()).ID);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Delete_AnotherUsersAlbum_ThrowsNotFound_AndChangesNothing()
    {
        EntityAlbum foreign = SeedAlbum("Theirs", Guid.CreateVersion7());

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _handler.Handle(new(UserID, foreign.ID), Token).AsTask());

        Assert.Single(_store.AlbumList);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Delete_WhenTheCommitFails_PublishesNothing()
    {
        EntityAlbum album = SeedAlbum("Record");
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new(UserID, album.ID), Token).AsTask());

        Assert.Empty(_publisher.PublishedList);
    }
}
