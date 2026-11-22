using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Genre;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerGenreListTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly HandlerGenreList _handler;
    private DateTime _now = Start;

    public HandlerGenreListTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new ETagService());
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityGenre Seed(string name, Guid? userID = null, int fileCount = 0)
    {
        _now = _now.AddSeconds(1);
        EntityGenre genre = new() { UserID = userID ?? UserID, Name = name, NameKey = name.ToLowerInvariant(), TimeCreated = _now, TimeModified = _now };
        _store.GenreList.Add(genre);

        for (int index = 0; index < fileCount; index++)
        {
            LinkNewFile(genre, userID ?? UserID);
        }

        return genre;
    }

    private void LinkNewFile(EntityGenre genre, Guid userID)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new('a', 64), UserID = userID };
        file.GenreLinkList = [new() { MediaFileID = file.ID, GenreID = genre.ID, Position = 0 }];
        _store.MediaFileList.Add(file);
    }

    private Task<MetadataLibrary<ResponseGenre>> List(string? ifNoneMatch = null)
    {
        return _handler.Handle(new(UserID, ifNoneMatch), Token).AsTask();
    }

    [Fact]
    public async Task Handle_ReturnsTheOwnersGenresNewestFirstWithTheirFileCounts()
    {
        Seed("Older", fileCount: 2);
        Seed("Newer");
        Seed("Foreign", Guid.CreateVersion7(), 5);

        MetadataLibrary<ResponseGenre> library = await List();

        Assert.NotNull(library.ItemList);
        Assert.Equal(["Newer", "Older"], library.ItemList.Select(x => x.Name).ToList());
        Assert.Equal([0, 2], library.ItemList.Select(x => x.MediaFileCount).ToList());
    }

    [Fact]
    public async Task Handle_ARenameOrACountChange_ChangesTheETag_AndAnotherUsersGenreDoesNot()
    {
        EntityGenre genre = Seed("A");
        string before = (await List()).ETag;

        Seed("Foreign", Guid.CreateVersion7(), 2);
        Assert.Equal(before, (await List()).ETag);

        LinkNewFile(genre, UserID);
        string afterCount = (await List()).ETag;
        Assert.NotEqual(before, afterCount);

        genre.TimeModified = genre.TimeModified.AddSeconds(5);
        Assert.NotEqual(afterCount, (await List()).ETag);
    }

    [Fact]
    public async Task Handle_AMatchingIfNoneMatch_ReturnsNoList_AndAnEmptyLibraryIsAnEmptyList()
    {
        MetadataLibrary<ResponseGenre> emptyLibrary = await List();
        Assert.NotNull(emptyLibrary.ItemList);
        Assert.Empty(emptyLibrary.ItemList);
        Assert.Null((await List(emptyLibrary.ETag)).ItemList);

        Seed("A");
        string eTag = (await List()).ETag;
        Assert.NotEqual(emptyLibrary.ETag, eTag);
        Assert.Null((await List(eTag)).ItemList);
    }
}
