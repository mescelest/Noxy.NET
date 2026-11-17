using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FakeArtistListStoreTest : ArtistListStoreContract
{
    protected override IListStore<ResponseArtist, Guid> CreateStore()
    {
        return new FakeMetadataStore().Artist;
    }
}
