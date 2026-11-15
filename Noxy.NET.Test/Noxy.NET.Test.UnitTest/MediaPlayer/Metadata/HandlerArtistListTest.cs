using Noxy.NET.MediaPlayer.Application.Handlers.Metadata;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerArtistListTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly HandlerArtistList _handler;
    private DateTime _now = Start;

    public HandlerArtistListTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new ETagService());
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityArtist Seed(string name, Guid? userID = null, int fileCount = 0)
    {
        _now = _now.AddSeconds(1);
        EntityArtist artist = new() { UserID = userID ?? UserID, Name = name, NameKey = name.ToLowerInvariant(), TimeCreated = _now, TimeModified = _now };
        _store.ArtistList.Add(artist);

        for (int index = 0; index < fileCount; index++)
        {
            LinkNewFile(artist, userID ?? UserID);
        }

        return artist;
    }

    private void LinkNewFile(EntityArtist artist, Guid userID)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new string('a', 64), UserID = userID };
        file.ArtistLinkList = [new() { MediaFileID = file.ID, ArtistID = artist.ID, Position = 0 }];
        _store.MediaFileList.Add(file);
    }

    private Task<MetadataLibrary<ResponseArtist>> List(string? ifNoneMatch = null)
    {
        return _handler.Handle(new(UserID, ifNoneMatch), Token).AsTask();
    }

    [Fact]
    public async Task Handle_ReturnsTheOwnersArtistsNewestFirstWithTheirFileCounts()
    {
        Seed("Older", fileCount: 2);
        Seed("Newer");
        Seed("Foreign", Guid.CreateVersion7(), 5);

        MetadataLibrary<ResponseArtist> library = await List();

        Assert.NotNull(library.ItemList);
        Assert.Equal(["Newer", "Older"], library.ItemList.Select(x => x.Name).ToList());
        Assert.Equal([0, 2], library.ItemList.Select(x => x.MediaFileCount).ToList());
        Assert.Equal(DateTimeKind.Utc, library.ItemList[0].TimeModified.Kind);
    }

    [Fact]
    public async Task Handle_TheETagIsStrongAndStable()
    {
        Seed("A");

        string first = (await List()).ETag;
        string second = (await List()).ETag;

        Assert.Equal(first, second);
        Assert.Matches("^\"[0-9a-f]{64}\"$", first);
    }

    [Fact]
    public async Task Handle_ARename_ChangesTheETag()
    {
        EntityArtist artist = Seed("A");
        string before = (await List()).ETag;

        artist.Name = "B";
        artist.TimeModified = artist.TimeModified.AddSeconds(5);

        Assert.NotEqual(before, (await List()).ETag);
    }

    [Fact]
    public async Task Handle_AnotherFileLinked_ChangesTheETagEvenThoughNoItemRowChanged()
    {
        EntityArtist artist = Seed("A");
        string before = (await List()).ETag;

        LinkNewFile(artist, UserID);

        Assert.NotEqual(before, (await List()).ETag);
    }

    [Fact]
    public async Task Handle_ItemsOfAnotherUser_DoNotChangeTheETag()
    {
        Seed("A");
        string before = (await List()).ETag;

        Seed("Foreign", Guid.CreateVersion7(), 3);

        Assert.Equal(before, (await List()).ETag);
    }

    [Fact]
    public async Task Handle_AMatchingIfNoneMatch_ReturnsNoList()
    {
        Seed("A");
        string eTag = (await List()).ETag;

        MetadataLibrary<ResponseArtist> library = await List(eTag);

        Assert.Equal(eTag, library.ETag);
        Assert.Null(library.ItemList);
    }

    [Fact]
    public async Task Handle_AStaleIfNoneMatch_ReturnsTheListAndTheNewETag()
    {
        EntityArtist artist = Seed("A");
        string stale = (await List()).ETag;
        artist.TimeModified = artist.TimeModified.AddSeconds(9);

        MetadataLibrary<ResponseArtist> library = await List(stale);

        Assert.NotEqual(stale, library.ETag);
        Assert.Single(library.ItemList ?? []);
    }

    [Fact]
    public async Task Handle_TheConditionalETag_EqualsTheETagOfTheFullList_AlsoWhenWeak()
    {
        Seed("A", fileCount: 1);
        Seed("B");
        string full = (await List()).ETag;

        Assert.Null((await List(full)).ItemList);
        Assert.Null((await List("W/" + full)).ItemList);
    }

    [Fact]
    public async Task Handle_NoArtists_IsAnEmptyListWithAStableETag()
    {
        MetadataLibrary<ResponseArtist> first = await List();
        MetadataLibrary<ResponseArtist> second = await List();

        Assert.NotNull(first.ItemList);
        Assert.Empty(first.ItemList);
        Assert.Equal(first.ETag, second.ETag);
        Assert.Null((await List(first.ETag)).ItemList);
    }
}
