using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Storage.Stores;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FileVolumeStoreTest : VolumeStoreContract, IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"Volume-file-{Guid.CreateVersion7()}");

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
    }

    protected override IVolumeStore CreateStore()
    {
        return new FileDeviceStore(Path.Combine(_folder, "device.json"));
    }

    [Fact]
    public async Task TheVolume_SurvivesANewStoreOverTheSameFile()
    {
        await CreateStore().SetVolume(42);

        Assert.Equal(42, await CreateStore().GetVolume());
    }
}
