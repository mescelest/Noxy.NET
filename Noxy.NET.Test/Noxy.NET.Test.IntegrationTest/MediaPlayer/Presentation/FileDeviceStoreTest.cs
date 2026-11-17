using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Storage.Stores;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FileDeviceStoreTest : DeviceStoreContract, IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"Device-file-{Guid.CreateVersion7()}");

    private string FilePath => Path.Combine(_folder, "device.json");

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
    }

    protected override IDeviceStore CreateStore()
    {
        return new FileDeviceStore(FilePath);
    }

    [Fact]
    public async Task TheIDAndName_SurviveANewStoreOverTheSameFile()
    {
        Guid id = Guid.NewGuid();
        FileDeviceStore first = new(FilePath);
        await first.SetDeviceID(id);
        await first.SetDeviceName("Laptop");

        FileDeviceStore second = new(FilePath);

        Assert.Equal(id, await second.GetDeviceID());
        Assert.Equal("Laptop", await second.GetDeviceName());
    }

    [Fact]
    public async Task TheVolume_AndTheDeviceValues_ShareTheFileWithoutReplacingEachOther()
    {
        FileDeviceStore store = new(FilePath);
        Guid id = Guid.NewGuid();

        await store.SetDeviceID(id);
        await store.SetVolume(35);
        await store.SetDeviceName("Phone");

        FileDeviceStore reopened = new(FilePath);
        Assert.Equal(id, await reopened.GetDeviceID());
        Assert.Equal("Phone", await reopened.GetDeviceName());
        Assert.Equal(35, await reopened.GetVolume());
        Assert.Equal([FilePath], Directory.GetFiles(_folder));
    }

    [Fact]
    public async Task ADamagedFile_ReadsAsAnEmptyDevice_AndCanBeWrittenAgain()
    {
        Directory.CreateDirectory(_folder);
        await File.WriteAllTextAsync(FilePath, "{\"deviceID\": ", TestContext.Current.CancellationToken);
        FileDeviceStore store = new(FilePath);

        Assert.Null(await store.GetDeviceID());
        Assert.Equal(100, await store.GetVolume());

        await store.SetVolume(20);
        Assert.Equal(20, await new FileDeviceStore(FilePath).GetVolume());
    }
}
