using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public abstract class DeviceStoreContract
{
    protected abstract IDeviceStore CreateStore();

    [Fact]
    public async Task ANewStore_HoldsNoIDAndNoName()
    {
        IDeviceStore store = CreateStore();

        Assert.Null(await store.GetDeviceID());
        Assert.Null(await store.GetDeviceName());
    }

    [Fact]
    public async Task TheID_RoundTrips()
    {
        IDeviceStore store = CreateStore();
        Guid id = Guid.NewGuid();

        await store.SetDeviceID(id);

        Assert.Equal(id, await store.GetDeviceID());
    }

    [Fact]
    public async Task TheName_RoundTrips_AndTheLastWriteWins()
    {
        IDeviceStore store = CreateStore();

        await store.SetDeviceName("Laptop");
        await store.SetDeviceName("Phone");

        Assert.Equal("Phone", await store.GetDeviceName());
    }
}
