using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public abstract class MetadataListStoreContract
{
    private static readonly DateTime Start = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    protected abstract IMetadataListStore CreateStore();

    [Fact]
    public async Task TheThreeKinds_NeverTouchEachOthersListsOrETags()
    {
        IMetadataListStore store = CreateStore();
        Guid sharedID = Guid.CreateVersion7();
        await store.Artist.ReplaceList([new() { ID = sharedID, Name = "Artist", MediaFileCount = 1, TimeModified = Start }]);
        await store.Album.ReplaceList([new() { ID = sharedID, Title = "Album", ArtistIDList = [sharedID], MediaFileCount = 1, TimeModified = Start }]);
        await store.Genre.ReplaceList([new() { ID = sharedID, Name = "Genre", MediaFileCount = 1, TimeModified = Start }]);
        await store.Artist.SetETag("\"a\"");
        await store.Album.SetETag("\"b\"");
        await store.Genre.SetETag("\"g\"");

        await store.Artist.Delete(sharedID);
        await store.Album.SetETag(null);

        Assert.Empty(await store.Artist.GetList());
        Assert.Single(await store.Album.GetList());
        Assert.Single(await store.Genre.GetList());
        Assert.Equal("\"a\"", await store.Artist.GetETag());
        Assert.Null(await store.Album.GetETag());
        Assert.Equal("\"g\"", await store.Genre.GetETag());
    }
}
