using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FakeFileStoreTest : FileStoreContract
{
    private readonly FakeFileStore _store = new();

    protected override IFileStore CreateStore()
    {
        return _store;
    }

    protected override Task<byte[]> ReadAtPath(string path)
    {
        return Task.FromResult(_store.ReadPath(path));
    }

    [Fact]
    public async Task WhenFull_AppendPartialThrowsStorageFullException_AndChangesNothing()
    {
        Guid held = Guid.CreateVersion7();
        _store.Hold(held, [1, 2, 3]);
        _store.IsFull = true;
        Guid id = Guid.CreateVersion7();

        await Assert.ThrowsAsync<StorageFullException>(() => _store.AppendPartial(id, new MemoryStream([1, 2, 3])));

        Assert.Equal([held], await _store.GetHeldIDList());
        Assert.Empty(await _store.GetPartialIDList());
    }

    [Fact]
    public async Task WhenTheLimitIsPassed_AppendPartialThrowsStorageFullException_AndKeepsWhatWasThere()
    {
        _store.FullAfterPartialBytes = 5;
        Guid id = Guid.CreateVersion7();
        await _store.AppendPartial(id, new MemoryStream([1, 2, 3]));

        await Assert.ThrowsAsync<StorageFullException>(() => _store.AppendPartial(id, new MemoryStream([4, 5, 6])));

        Assert.Equal(3, await _store.GetPartialLength(id));
    }
}
