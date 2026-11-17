using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FakeAlbumListStoreTest : AlbumListStoreContract
{
    protected override IListStore<ResponseAlbum, Guid> CreateStore()
    {
        return new FakeMetadataStore().Album;
    }
}
