using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FakeMetadataStoreTest : MetadataStoreContract
{
    protected override IMetadataStore CreateStore()
    {
        return new FakeMetadataStore();
    }
}
