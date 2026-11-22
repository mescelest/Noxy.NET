using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerAlbumMergeTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerAlbumMerge _handler;

    public HandlerAlbumMergeTest()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);
        _handler = new(new FakeUnitOfWorkFactory(_store), new NameKeyService(), new MetadataChangeService(), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityArtist SeedArtist(string name)
    {
        EntityArtist artist = new() { UserID = UserID, Name = name, NameKey = name.ToLowerInvariant(), TimeModified = Original };
        _store.ArtistList.Add(artist);
        return artist;
    }

    private EntityAlbum SeedAlbum(string title, Guid? userID = null, params EntityArtist[] artistList)
    {
        EntityAlbum album = new() { UserID = userID ?? UserID, Title = title, TitleKey = title.ToLowerInvariant(), IdentityKey = EntityAlbum.BuildIdentityKey(title.ToLowerInvariant(), artistList.Select(x => x.ID)), TimeModified = Original };
        album.ArtistLinkList = [.. artistList.Select((x, index) => new EntityAlbumArtist { AlbumID = album.ID, ArtistID = x.ID, Position = index })];
        _store.AlbumList.Add(album);
        return album;
    }

    private EntityMediaFile SeedFile(string title, EntityAlbum album)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new('a', 64), UserID = UserID, TimeModified = Original, AlbumID = album.ID };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    [Fact]
    public async Task Merge_MovesTheFilesToTheTargetWhichKeepsItsTitleAndArtists_AndRemovesTheSource()
    {
        EntityArtist sourceArtist = SeedArtist("Source artist");
        EntityArtist targetArtist = SeedArtist("Target artist");
        EntityAlbum source = SeedAlbum("Source", null, sourceArtist);
        EntityAlbum target = SeedAlbum("Target", null, targetArtist);
        EntityMediaFile moving = SeedFile("One", source);
        EntityMediaFile staying = SeedFile("Two", target);

        ResponseAlbum response = await _handler.Handle(new(UserID, source.ID, target.ID), Token);

        Assert.Equal(target.ID, response.ID);
        Assert.Equal("Target", response.Title);
        Assert.Equal([targetArtist.ID], response.ArtistIDList);
        Assert.Equal(2, response.MediaFileCount);
        Assert.Equal(target.ID, Assert.Single(_store.AlbumList).ID);
        Assert.Equal(target.ID, Stored(moving).AlbumID);
        Assert.Equal(_time.Now.UtcDateTime, Stored(moving).TimeModified);
        Assert.Equal(Original, Stored(staying).TimeModified);
        NotificationMediaUpdated pushed = Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal("Target", pushed.MediaFile.Album);
        Assert.Equal("Target artist", pushed.MediaFile.AlbumArtist);
        Assert.Equal(source.ID, Assert.Single(_publisher.GetList<NotificationAlbumDeleted>()).ID);
        Assert.Equal(target.ID, Assert.Single(_publisher.GetList<NotificationAlbumUpdated>()).Album.ID);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Merge_IntoItselfIsInvalid_AndAnotherUsersAlbumIsNotFound()
    {
        EntityAlbum mine = SeedAlbum("Mine");
        EntityAlbum foreign = SeedAlbum("Theirs", Guid.CreateVersion7());

        await Assert.ThrowsAsync<InvalidMetadataOperationException>(() => _handler.Handle(new(UserID, mine.ID, mine.ID), Token).AsTask());
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _handler.Handle(new(UserID, mine.ID, foreign.ID), Token).AsTask());
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _handler.Handle(new(UserID, foreign.ID, mine.ID), Token).AsTask());

        Assert.Equal(2, _store.AlbumList.Count);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Merge_WhenTheCommitFails_PublishesNothing()
    {
        EntityAlbum source = SeedAlbum("Source");
        EntityAlbum target = SeedAlbum("Target");
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new(UserID, source.ID, target.ID), Token).AsTask());

        Assert.Empty(_publisher.PublishedList);
    }
}
