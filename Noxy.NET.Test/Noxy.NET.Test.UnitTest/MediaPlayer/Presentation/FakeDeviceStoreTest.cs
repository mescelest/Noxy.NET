using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FakeDeviceStoreTest : DeviceStoreContract
{
    protected override IDeviceStore CreateStore()
    {
        return new FakeDeviceStore();
    }
}
