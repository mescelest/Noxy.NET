using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public abstract class ArtistListStoreContract : ListStoreContract<ResponseArtist, Guid>
{
    protected override Guid NewKey()
    {
        return Guid.CreateVersion7();
    }

    protected override ResponseArtist MakeItem(string label, Guid key)
    {
        return new() { ID = key, Name = label, MediaFileCount = 3, TimeModified = Start };
    }

    protected override Guid KeyOf(ResponseArtist item)
    {
        return item.ID;
    }

    protected override string LabelOf(ResponseArtist item)
    {
        return item.Name;
    }

    [Fact]
    public async Task EveryFieldComesBackEqual()
    {
        IListStore<ResponseArtist, Guid> store = CreateStore();
        ResponseArtist artist = new() { ID = Guid.CreateVersion7(), Name = "Ünïcödé \"Artist\"", MediaFileCount = 0, TimeModified = Start.AddMinutes(7) };

        await store.Save(artist);

        ResponseArtist stored = Assert.Single(await store.GetList());
        Assert.Equal((artist.ID, artist.Name, artist.MediaFileCount, artist.TimeModified), (stored.ID, stored.Name, stored.MediaFileCount, stored.TimeModified));
    }
}
