using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Storage.Stores;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FileLocalQueueStoreTest : LocalQueueStoreContract, IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"Local-queue-file-{Guid.CreateVersion7()}");

    private string FilePath => Path.Combine(_folder, "queue.json");

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
    }

    protected override ILocalQueueStore CreateStore()
    {
        return new FileLocalQueueStore(FilePath);
    }

    [Fact]
    public async Task TheCopy_SurvivesANewStoreOverTheSameFile()
    {
        LocalQueueSnapshot snapshot = new(new() { QueueRevision = 3, Source = null, HistoryList = [], Current = new(Guid.NewGuid(), Guid.NewGuid()), UserQueueList = [], SourceList = [] }, 7, DateTime.UtcNow);
        await CreateStore().Save(snapshot);

        LocalQueueSnapshot? loaded = await CreateStore().Get();

        Assert.Equal(snapshot.Queue.Current, loaded?.Queue.Current);
        Assert.Equal(7, loaded?.PositionSeconds);
    }

    [Fact]
    public async Task ADamagedFile_IsDiscarded_AndReadsAsNothingSaved()
    {
        Directory.CreateDirectory(_folder);
        await File.WriteAllTextAsync(FilePath, "{\"queue\": {", TestContext.Current.CancellationToken);

        Assert.Null(await CreateStore().Get());
        Assert.False(File.Exists(FilePath));
    }

    [Fact]
    public async Task ASave_LeavesNoTemporaryFileBehind()
    {
        await CreateStore().Save(new(new() { QueueRevision = 1, Source = null, HistoryList = [], Current = null, UserQueueList = [], SourceList = [] }, 0, DateTime.UtcNow));

        Assert.Equal([FilePath], Directory.GetFiles(_folder));
    }
}
