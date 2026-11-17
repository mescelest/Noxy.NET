using Microsoft.Data.Sqlite;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Storage.Stores;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class SqliteLocalQueueStoreTest : LocalQueueStoreContract, IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"Local-queue-store-{Guid.CreateVersion7()}");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
    }

    protected override ILocalQueueStore CreateStore()
    {
        return new SqliteMetadataStore(Path.Combine(_folder, "metadata.db"));
    }
}
