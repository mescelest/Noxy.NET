using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public abstract class VolumeStoreContract
{
    protected abstract IVolumeStore CreateStore();

    [Fact]
    public async Task ANewStore_HoldsFullVolume()
    {
        IVolumeStore store = CreateStore();

        Assert.Equal(100, await store.GetVolume());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(35)]
    [InlineData(100)]
    public async Task AVolume_RoundTrips(int volume)
    {
        IVolumeStore store = CreateStore();

        await store.SetVolume(volume);

        Assert.Equal(volume, await store.GetVolume());
    }

    [Fact]
    public async Task TheLastWriteWins()
    {
        IVolumeStore store = CreateStore();

        await store.SetVolume(10);
        await store.SetVolume(80);

        Assert.Equal(80, await store.GetVolume());
    }
}
