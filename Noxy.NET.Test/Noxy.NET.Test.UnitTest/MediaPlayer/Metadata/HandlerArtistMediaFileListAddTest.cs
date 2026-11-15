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

public class HandlerArtistMediaFileListAddTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerArtistMediaFileListAdd _handler;

    public HandlerArtistMediaFileListAddTest()
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

    private Task<ResponseArtist> Add(EntityArtist artist, params EntityMediaFile[] fileList)
    {
        return _handler.Handle(new CommandArtistMediaFileListAdd(UserID, artist.ID, [.. fileList.Select(x => x.ID)]), Token).AsTask();
    }

    [Fact]
    public async Task Add_PutsTheArtistAtTheEndOfTheArtistsOfEachFile()
    {
        EntityArtist artist = SeedArtist("New");
        EntityArtist existing = SeedArtist("Existing");
        EntityMediaFile first = SeedFile("One", null, existing);
        EntityMediaFile second = SeedFile("Two");

        ResponseArtist response = await Add(artist, first, second);

        Assert.Equal([existing.ID, artist.ID], Stored(first).ArtistLinkList.OrderBy(x => x.Position).Select(x => x.ArtistID).ToList());
        Assert.Equal([artist.ID], Stored(second).ArtistLinkList.Select(x => x.ArtistID).ToList());
        Assert.Equal(2, response.MediaFileCount);
    }

    [Fact]
    public async Task Add_DoesNotDuplicateALinkTheFileAlreadyHas()
    {
        EntityArtist artist = SeedArtist("A");
        EntityMediaFile had = SeedFile("One", null, artist);
        EntityMediaFile lacking = SeedFile("Two");

        ResponseArtist response = await Add(artist, had, lacking);

        Assert.Single(Stored(had).ArtistLinkList);
        Assert.Equal(Original, Stored(had).TimeModified);
        Assert.Equal(_time.Now.UtcDateTime, Stored(lacking).TimeModified);
        Assert.Equal(2, response.MediaFileCount);
        Assert.Equal(lacking.ID, Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.ID);
    }

    [Fact]
    public async Task Add_TouchesAndPushesTheChangedFilesAndTheArtistAfterTheCommit()
    {
        EntityArtist artist = SeedArtist("A");
        EntityMediaFile file = SeedFile("One");
        EntityMediaFile unrelated = SeedFile("Two");
        int commitsAtPublish = -1;
        _publisher.OnPublish = _ => commitsAtPublish = _store.CommitCount;

        await Add(artist, file);

        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        Assert.Equal(Original, Stored(unrelated).TimeModified);
        Assert.Equal("A", Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.Artist);
        Assert.Equal(artist.ID, Assert.Single(_publisher.GetList<NotificationArtistUpdated>()).Artist.ID);
        Assert.Equal(1, _store.CommitCount);
        Assert.Equal(1, commitsAtPublish);
    }

    [Fact]
    public async Task Add_WhatIsAlreadyThere_ChangesNothingCommitsNothingAndPushesNothing()
    {
        EntityArtist artist = SeedArtist("A");
        EntityMediaFile file = SeedFile("One", null, artist);

        ResponseArtist response = await Add(artist, file);

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
        Assert.Equal(Original, Stored(file).TimeModified);
        Assert.Equal(1, response.MediaFileCount);
    }

    [Fact]
    public async Task Add_AnUnknownFile_IsNotFound_AndNothingChanges()
    {
        EntityArtist artist = SeedArtist("A");
        EntityMediaFile valid = SeedFile("One");

        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new CommandArtistMediaFileListAdd(UserID, artist.ID, [valid.ID, Guid.CreateVersion7()]), Token).AsTask());

        Assert.Empty(Stored(valid).ArtistLinkList);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Add_AnotherUsersFile_IsNotFound()
    {
        EntityArtist artist = SeedArtist("A");
        EntityMediaFile foreign = SeedFile("Theirs", Guid.CreateVersion7());

        await Assert.ThrowsAsync<MediaNotFoundException>(() => Add(artist, foreign));

        Assert.Empty(Stored(foreign).ArtistLinkList);
    }

    [Fact]
    public async Task Add_AnotherUsersArtist_IsNotFound()
    {
        EntityArtist foreign = SeedArtist("Theirs", Guid.CreateVersion7());
        EntityMediaFile file = SeedFile("One");

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Add(foreign, file));
    }

    [Fact]
    public async Task Add_TheSameFileTwice_IsLinkedOnce()
    {
        EntityArtist artist = SeedArtist("A");
        EntityMediaFile file = SeedFile("One");

        ResponseArtist response = await _handler.Handle(new CommandArtistMediaFileListAdd(UserID, artist.ID, [file.ID, file.ID]), Token);

        Assert.Single(Stored(file).ArtistLinkList);
        Assert.Equal(1, response.MediaFileCount);
    }

    [Fact]
    public async Task Add_WhenTheCommitFails_PublishesNothing()
    {
        EntityArtist artist = SeedArtist("A");
        EntityMediaFile file = SeedFile("One");
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Add(artist, file));

        Assert.Empty(_publisher.PublishedList);
    }
}
