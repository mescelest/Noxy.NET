using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FakeVolumeStoreTest : VolumeStoreContract
{
    protected override IVolumeStore CreateStore()
    {
        return new FakeMetadataStore();
    }
}
