using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FakeMetadataListStoreTest : MetadataListStoreContract
{
    protected override IMetadataListStore CreateStore()
    {
        return new FakeMetadataStore();
    }
}
