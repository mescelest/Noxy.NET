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

public class HandlerArtistMediaFileListRemoveTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerArtistMediaFileListRemove _handler;

    public HandlerArtistMediaFileListRemoveTest()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);
        _handler = new(new FakeUnitOfWorkFactory(_store), new ArtistItemService(new MetadataChangeService()), new NameKeyService(), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityArtist SeedArtist(string name)
    {
        EntityArtist artist = new() { UserID = UserID, Name = name, NameKey = name.ToLowerInvariant(), TimeModified = Original };
        _store.ArtistList.Add(artist);
        return artist;
    }

    private EntityMediaFile SeedFile(string title, Guid? userID = null, params EntityArtist[] artistList)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new string('a', 64), UserID = userID ?? UserID, TimeModified = Original };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        file.ArtistLinkList = [.. artistList.Select((x, index) => new EntityMediaFileArtist { MediaFileID = file.ID, ArtistID = x.ID, Position = index })];
        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    private Task<ResponseArtist> Remove(EntityArtist artist, params EntityMediaFile[] fileList)
    {
        return _handler.Handle(new CommandArtistMediaFileListRemove(UserID, artist.ID, [.. fileList.Select(x => x.ID)]), Token).AsTask();
    }

    [Fact]
    public async Task Remove_ClearsOnlyThatArtistFromTheFilesAndKeepsTheOthers()
    {
        EntityArtist artist = SeedArtist("A");
        EntityArtist other = SeedArtist("B");
        EntityMediaFile file = SeedFile("One", null, other, artist);
        EntityMediaFile untouched = SeedFile("Two", null, artist);

        ResponseArtist response = await Remove(artist, file);

        Assert.Equal([other.ID], Stored(file).ArtistLinkList.Select(x => x.ArtistID).ToList());
        Assert.Equal([artist.ID], Stored(untouched).ArtistLinkList.Select(x => x.ArtistID).ToList());
        Assert.Equal(Original, Stored(untouched).TimeModified);
        Assert.Equal(1, response.MediaFileCount);
        Assert.Contains(_store.ArtistList, x => x.ID == artist.ID);
    }

    [Fact]
    public async Task Remove_TouchesAndPushesTheChangedFilesAndTheArtistAfterTheCommit()
    {
        EntityArtist artist = SeedArtist("A");
        EntityMediaFile file = SeedFile("One", null, artist);
        int commitsAtPublish = -1;
        _publisher.OnPublish = _ => commitsAtPublish = _store.CommitCount;

        await Remove(artist, file);

        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        Assert.Null(Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.Artist);
        Assert.Equal(0, Assert.Single(_publisher.GetList<NotificationArtistUpdated>()).Artist.MediaFileCount);
        Assert.Equal(1, _store.CommitCount);
        Assert.Equal(1, commitsAtPublish);
    }

    [Fact]
    public async Task Remove_AFileThatDoesNotHaveTheArtist_ChangesNothingAndPushesNothing()
    {
        EntityArtist artist = SeedArtist("A");
        EntityMediaFile file = SeedFile("One");

        await Remove(artist, file);

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
        Assert.Equal(Original, Stored(file).TimeModified);
    }

    [Fact]
    public async Task Remove_AnUnknownOrForeignFile_IsNotFound_AndNothingChanges()
    {
        EntityArtist artist = SeedArtist("A");
        EntityMediaFile valid = SeedFile("One", null, artist);
        EntityMediaFile foreign = SeedFile("Theirs", Guid.CreateVersion7(), artist);

        await Assert.ThrowsAsync<MediaNotFoundException>(() => Remove(artist, valid, foreign));
        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new CommandArtistMediaFileListRemove(UserID, artist.ID, [Guid.CreateVersion7()]), Token).AsTask());

        Assert.Single(Stored(valid).ArtistLinkList);
        Assert.Single(Stored(foreign).ArtistLinkList);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Remove_AnotherUsersArtist_IsNotFound()
    {
        EntityArtist foreign = new() { UserID = Guid.CreateVersion7(), Name = "Theirs", NameKey = "theirs" };
        _store.ArtistList.Add(foreign);
        EntityMediaFile file = SeedFile("One");

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Remove(foreign, file));
    }
}
