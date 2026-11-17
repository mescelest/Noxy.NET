using Microsoft.Data.Sqlite;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Storage.Stores;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class SqliteDeviceStoreTest : DeviceStoreContract, IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"Device-store-{Guid.CreateVersion7()}");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
    }

    protected override IDeviceStore CreateStore()
    {
        return new SqliteMetadataStore(Path.Combine(_folder, "metadata.db"));
    }

    [Fact]
    public async Task TheIDAndName_SurviveANewStoreOverTheSameFile()
    {
        Guid id = Guid.NewGuid();
        IDeviceStore first = CreateStore();
        await first.SetDeviceID(id);
        await first.SetDeviceName("Laptop");

        IDeviceStore second = CreateStore();

        Assert.Equal(id, await second.GetDeviceID());
        Assert.Equal("Laptop", await second.GetDeviceName());
    }
}
