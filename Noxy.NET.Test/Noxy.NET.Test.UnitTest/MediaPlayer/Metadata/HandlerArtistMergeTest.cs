using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Artist;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerArtistMergeTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerArtistMerge _handler;

    public HandlerArtistMergeTest()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);
        _handler = new(new FakeUnitOfWorkFactory(_store), new ArtistItemService(new MetadataChangeService()), new NameKeyService(), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityArtist SeedArtist(string name, Guid? userID = null)
    {
        EntityArtist artist = new() { UserID = userID ?? UserID, Name = name, NameKey = name.ToLowerInvariant(), TimeModified = Original };
        _store.ArtistList.Add(artist);
        return artist;
    }

    private EntityMediaFile SeedFile(string title, EntityAlbum? album = null, params EntityArtist[] artistList)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new string('a', 64), UserID = UserID, TimeModified = Original, AlbumID = album?.ID };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        file.ArtistLinkList = [.. artistList.Select((x, index) => new EntityMediaFileArtist { MediaFileID = file.ID, ArtistID = x.ID, Position = index })];
        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityAlbum SeedAlbum(string title, params EntityArtist[] artistList)
    {
        EntityAlbum album = new() { UserID = UserID, Title = title, TitleKey = title.ToLowerInvariant(), IdentityKey = EntityAlbum.BuildIdentityKey(title.ToLowerInvariant(), artistList.Select(x => x.ID)), TimeModified = Original };
        album.ArtistLinkList = [.. artistList.Select((x, index) => new EntityAlbumArtist { AlbumID = album.ID, ArtistID = x.ID, Position = index })];
        _store.AlbumList.Add(album);
        return album;
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    private Task<ResponseArtist> Merge(EntityArtist source, EntityArtist target)
    {
        return _handler.Handle(new CommandArtistMerge(UserID, source.ID, target.ID), Token).AsTask();
    }

    [Fact]
    public async Task Merge_MovesEveryLinkOfTheSourceToTheTargetWithoutDuplicatesAndRemovesTheSource()
    {
        EntityArtist source = SeedArtist("Source");
        EntityArtist target = SeedArtist("Target");
        EntityArtist other = SeedArtist("Other");
        EntityMediaFile onlySource = SeedFile("One", null, other, source);
        EntityMediaFile both = SeedFile("Two", null, source, target);
        EntityMediaFile onlyTarget = SeedFile("Three", null, target);

        ResponseArtist response = await Merge(source, target);

        Assert.Equal(target.ID, response.ID);
        Assert.Equal("Target", response.Name);
        Assert.Equal(3, response.MediaFileCount);
        Assert.DoesNotContain(_store.ArtistList, x => x.ID == source.ID);
        Assert.Equal([other.ID, target.ID], Stored(onlySource).ArtistLinkList.OrderBy(x => x.Position).Select(x => x.ArtistID).ToList());
        Assert.Equal([target.ID], Stored(both).ArtistLinkList.Select(x => x.ArtistID).ToList());
        Assert.Equal([target.ID], Stored(onlyTarget).ArtistLinkList.Select(x => x.ArtistID).ToList());
    }

    [Fact]
    public async Task Merge_TouchesAndPushesExactlyTheFilesThatChanged_AndPublishesTheDeletedSourceAndTheUpdatedTarget()
    {
        EntityArtist source = SeedArtist("Source");
        EntityArtist target = SeedArtist("Target");
        EntityMediaFile moved = SeedFile("One", null, source);
        EntityMediaFile untouched = SeedFile("Two", null, target);
        int commitsAtPublish = -1;
        _publisher.OnPublish = _ => commitsAtPublish = _store.CommitCount;

        await Merge(source, target);

        Assert.Equal(_time.Now.UtcDateTime, Stored(moved).TimeModified);
        Assert.Equal(Original, Stored(untouched).TimeModified);
        Assert.Equal(moved.ID, Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.ID);
        Assert.Equal(source.ID, Assert.Single(_publisher.GetList<NotificationArtistDeleted>()).ID);
        Assert.Equal(target.ID, Assert.Single(_publisher.GetList<NotificationArtistUpdated>()).Artist.ID);
        Assert.Equal(1, _store.CommitCount);
        Assert.Equal(1, commitsAtPublish);
    }

    [Fact]
    public async Task Merge_ReplacesTheSourceInTheAlbumArtistListsToo_AndTouchesTheAlbumFiles()
    {
        EntityArtist source = SeedArtist("Source");
        EntityArtist target = SeedArtist("Target");
        EntityAlbum album = SeedAlbum("Record", source);
        EntityMediaFile file = SeedFile("Track", album);

        await Merge(source, target);

        EntityAlbum stored = Assert.Single(_store.AlbumList);
        Assert.Equal([target.ID], stored.ArtistLinkList.Select(x => x.ArtistID).ToList());
        Assert.Equal(EntityAlbum.BuildIdentityKey("record", [target.ID]), stored.IdentityKey);
        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        Assert.Equal("Target", Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.AlbumArtist);
    }

    [Fact]
    public async Task Merge_TwoAlbumsThatBecomeTheSame_AreMerged_AndTheFilesFollow()
    {
        EntityArtist source = SeedArtist("Source");
        EntityArtist target = SeedArtist("Target");
        EntityAlbum sourceAlbum = SeedAlbum("Hits", source);
        EntityAlbum targetAlbum = SeedAlbum("Hits", target);
        EntityMediaFile moving = SeedFile("One", sourceAlbum);
        EntityMediaFile staying = SeedFile("Two", targetAlbum);

        await Merge(source, target);

        Assert.Equal(targetAlbum.ID, Assert.Single(_store.AlbumList).ID);
        Assert.Equal(targetAlbum.ID, Stored(moving).AlbumID);
        Assert.Equal(Original, Stored(staying).TimeModified);
        Assert.Equal(sourceAlbum.ID, Assert.Single(_publisher.GetList<NotificationAlbumDeleted>()).ID);
    }

    [Fact]
    public async Task Merge_IntoItself_IsInvalid_AndChangesNothing()
    {
        EntityArtist artist = SeedArtist("A");

        await Assert.ThrowsAsync<InvalidMetadataOperationException>(() => Merge(artist, artist));

        Assert.Single(_store.ArtistList);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Merge_WithAnotherUsersArtistOnEitherSide_IsNotFound()
    {
        EntityArtist mine = SeedArtist("Mine");
        EntityArtist foreign = SeedArtist("Theirs", Guid.CreateVersion7());

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Merge(mine, foreign));
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Merge(foreign, mine));

        Assert.Equal(2, _store.ArtistList.Count);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Merge_WhenTheCommitFails_PublishesNothing()
    {
        EntityArtist source = SeedArtist("Source");
        EntityArtist target = SeedArtist("Target");
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Merge(source, target));

        Assert.Empty(_publisher.PublishedList);
    }
}
