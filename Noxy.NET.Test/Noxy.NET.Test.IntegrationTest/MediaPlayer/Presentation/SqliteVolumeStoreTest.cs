using Microsoft.Data.Sqlite;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Storage.Stores;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class SqliteVolumeStoreTest : VolumeStoreContract, IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"Volume-store-{Guid.CreateVersion7()}");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
    }

    protected override IVolumeStore CreateStore()
    {
        return new SqliteMetadataStore(Path.Combine(_folder, "metadata.db"));
    }

    [Fact]
    public async Task TheVolume_SurvivesANewStoreOverTheSameFile()
    {
        await CreateStore().SetVolume(42);

        Assert.Equal(42, await CreateStore().GetVolume());
    }
}
