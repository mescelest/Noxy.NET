using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FakeLocalQueueStoreTest : LocalQueueStoreContract
{
    protected override ILocalQueueStore CreateStore()
    {
        return new FakeLocalQueueStore();
    }
}
