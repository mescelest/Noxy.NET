using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public abstract class AlbumListStoreContract : ListStoreContract<ResponseAlbum, Guid>
{
    protected override Guid NewKey()
    {
        return Guid.CreateVersion7();
    }

    protected override ResponseAlbum MakeItem(string label, Guid key)
    {
        return new() { ID = key, Title = label, ArtistIDList = [Guid.CreateVersion7(), Guid.CreateVersion7()], MediaFileCount = 3, TimeModified = Start };
    }

    protected override Guid KeyOf(ResponseAlbum item)
    {
        return item.ID;
    }

    protected override string LabelOf(ResponseAlbum item)
    {
        return item.Title;
    }

    [Fact]
    public async Task EveryFieldComesBackEqual_IncludingTwoArtistIDs()
    {
        IListStore<ResponseAlbum, Guid> store = CreateStore();
        ResponseAlbum album = new() { ID = Guid.CreateVersion7(), Title = "Ünïcödé", ArtistIDList = [Guid.CreateVersion7(), Guid.CreateVersion7()], MediaFileCount = 0, TimeModified = Start.AddMinutes(7) };

        await store.Save(album);

        ResponseAlbum stored = Assert.Single(await store.GetList());
        Assert.Equal(album.ID, stored.ID);
        Assert.Equal(album.Title, stored.Title);
        Assert.Equal(album.ArtistIDList, stored.ArtistIDList);
        Assert.Equal(0, stored.MediaFileCount);
        Assert.Equal(album.TimeModified, stored.TimeModified);
    }
}
