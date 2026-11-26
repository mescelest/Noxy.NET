using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public abstract class FileStoreContract
{
    private const string Mp3 = "audio/mpeg";

    protected abstract IFileStore CreateStore();

    protected virtual async Task<byte[]> ReadAtPath(string path)
    {
        return await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);
    }

    private static byte[] Bytes(int length, byte seed = 1)
    {
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++) bytes[i] = (byte)(i * 7 + seed);

        return bytes;
    }

    private static async Task<byte[]> ReadAll(Stream? stream)
    {
        Assert.NotNull(stream);
        await using Stream open = stream;
        using MemoryStream copy = new();
        await open.CopyToAsync(copy, TestContext.Current.CancellationToken);

        return copy.ToArray();
    }

    private static async Task Hold(IFileStore store, Guid id, byte[] bytes, string contentType = Mp3)
    {
        await store.AppendPartial(id, new MemoryStream(bytes));
        await store.Commit(id, contentType);
    }

    [Fact]
    public async Task AnEmptyStore_HoldsNothing()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();

        Assert.Null(await store.GetPath(id));
        Assert.Null(await store.Open(id));
        Assert.Equal(0, await store.GetPartialLength(id));
        Assert.Empty(await store.GetHeldIDList());
        Assert.Empty(await store.GetPartialIDList());
        Assert.Equal(0, (await store.GetEstimate()).UsedBytes);
    }

    [Fact]
    public async Task ACommittedFile_IsHeld_WithExactlyTheCommittedBytesAtItsPath()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        byte[] bytes = Bytes(5000);

        await Hold(store, id, bytes);

        string? path = await store.GetPath(id);
        Assert.NotNull(path);
        Assert.Equal(bytes, await ReadAtPath(path));
        Assert.Equal(bytes, await ReadAll(await store.Open(id)));
        Assert.Equal([id], await store.GetHeldIDList());
    }

    [Fact]
    public async Task AppendingTwice_BuildsOnePartialFile_WhoseLengthIsTheSum()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();

        await store.AppendPartial(id, new MemoryStream(Bytes(300)));
        await store.AppendPartial(id, new MemoryStream(Bytes(200, 9)));

        Assert.Equal(500, await store.GetPartialLength(id));
        Assert.Equal([id], await store.GetPartialIDList());
    }

    [Fact]
    public async Task APartialFile_IsNotHeld_AndHasNoPath()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();

        await store.AppendPartial(id, new MemoryStream(Bytes(100)));

        Assert.Null(await store.GetPath(id));
        Assert.Null(await store.Open(id));
        Assert.Empty(await store.GetHeldIDList());
    }

    [Fact]
    public async Task Commit_MovesThePartialIntoTheStore_AndThePartialIsGone()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await store.AppendPartial(id, new MemoryStream(Bytes(100)));

        await store.Commit(id, Mp3);

        Assert.Equal(0, await store.GetPartialLength(id));
        Assert.Empty(await store.GetPartialIDList());
        Assert.Equal([id], await store.GetHeldIDList());
    }

    [Fact]
    public async Task CommittingTheSameIDTwice_ReplacesTheFile()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await Hold(store, id, Bytes(100));
        byte[] second = Bytes(150, 5);

        await Hold(store, id, second);

        Assert.Equal(second, await ReadAll(await store.Open(id)));
        Assert.Equal([id], await store.GetHeldIDList());
    }

    [Fact]
    public async Task CommittingTheSameIDAgainWithAnotherContentType_LeavesOnlyOneFile()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await Hold(store, id, Bytes(100));

        await Hold(store, id, Bytes(100, 3), "audio/flac");

        Assert.Equal([id], await store.GetHeldIDList());
        Assert.EndsWith(".flac", await store.GetPath(id));
    }

    [Fact]
    public async Task Delete_RemovesTheFile_AndAnUnknownIDIsHarmless()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await Hold(store, id, Bytes(100));

        await store.Delete(Guid.CreateVersion7());
        await store.Delete(id);

        Assert.Null(await store.GetPath(id));
        Assert.Null(await store.Open(id));
        Assert.Empty(await store.GetHeldIDList());
    }

    [Fact]
    public async Task DiscardPartial_RemovesIt_AndAnUnknownIDIsHarmless()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await store.AppendPartial(id, new MemoryStream(Bytes(100)));

        await store.DiscardPartial(Guid.CreateVersion7());
        await store.DiscardPartial(id);

        Assert.Equal(0, await store.GetPartialLength(id));
        Assert.Empty(await store.GetPartialIDList());
    }

    [Fact]
    public async Task TheLists_HoldExactlyTheHeldAndTheirPartialIDs()
    {
        IFileStore store = CreateStore();
        Guid held = Guid.CreateVersion7();
        Guid partial = Guid.CreateVersion7();
        await Hold(store, held, Bytes(100));
        await store.AppendPartial(partial, new MemoryStream(Bytes(50)));

        Assert.Equal([held], await store.GetHeldIDList());
        Assert.Equal([partial], await store.GetPartialIDList());
    }

    [Fact]
    public async Task GetEstimate_UsedBytesGrowByTheCommittedSize_AndPartialFilesDoNotCount()
    {
        IFileStore store = CreateStore();
        await Hold(store, Guid.CreateVersion7(), Bytes(1000));
        long before = (await store.GetEstimate()).UsedBytes;

        await store.AppendPartial(Guid.CreateVersion7(), new MemoryStream(Bytes(400)));
        Assert.Equal(before, (await store.GetEstimate()).UsedBytes);

        await Hold(store, Guid.CreateVersion7(), Bytes(2500));

        Assert.Equal(before + 2500, (await store.GetEstimate()).UsedBytes);
        Assert.True((await store.GetEstimate()).FreeBytes > 0);
    }

    [Fact]
    public async Task Commit_WithNoPartialFile_ThrowsInvalidOperationException()
    {
        IFileStore store = CreateStore();

        await Assert.ThrowsAsync<InvalidOperationException>(() => store.Commit(Guid.CreateVersion7(), Mp3));
    }

    [Fact]
    public async Task Rewrite_ReplacesTheHeldBytesWithWhatWriteLeft()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await Hold(store, id, Bytes(1000));
        byte[] rewritten = Bytes(1200, 9);

        await store.Rewrite(id, path => File.WriteAllBytes(path, rewritten));

        Assert.Equal(rewritten, await ReadAll(await store.Open(id)));
    }

    [Fact]
    public async Task Rewrite_StartsFromACopyOfTheHeldFile()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        byte[] original = Bytes(1000);
        await Hold(store, id, original);
        byte[]? seen = null;

        await store.Rewrite(id, path => seen = File.ReadAllBytes(path));

        Assert.Equal(original, seen);
    }

    [Fact]
    public async Task Rewrite_PassesAPathWithTheHeldFilesExtension()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await Hold(store, id, Bytes(100), "audio/flac");
        string? passed = null;

        await store.Rewrite(id, path => passed = path);

        Assert.Equal(".flac", Path.GetExtension(passed));
    }

    [Fact]
    public async Task Rewrite_WhenWriteThrows_KeepsTheHeldBytes_AndRethrows()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        byte[] original = Bytes(1000);
        await Hold(store, id, original);

        await Assert.ThrowsAsync<IOException>(() => store.Rewrite(id, path =>
        {
            File.WriteAllBytes(path, Bytes(50, 3));
            throw new IOException("The file is locked.");
        }));

        Assert.Equal(original, await ReadAll(await store.Open(id)));
    }

    [Fact]
    public async Task Rewrite_WhenWriteThrows_LeavesNoTemporaryFile()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await Hold(store, id, Bytes(1000));
        string? passed = null;

        await Assert.ThrowsAsync<IOException>(() => store.Rewrite(id, path =>
        {
            passed = path;
            throw new IOException("The file is locked.");
        }));

        Assert.False(File.Exists(passed));
        Assert.Equal([id], await store.GetHeldIDList());
    }

    [Fact]
    public async Task Rewrite_OfAFileThatIsNotHeld_Throws_WithoutCallingWrite()
    {
        IFileStore store = CreateStore();
        bool called = false;

        await Assert.ThrowsAsync<InvalidOperationException>(() => store.Rewrite(Guid.CreateVersion7(), _ => called = true));

        Assert.False(called);
    }

    [Fact]
    public async Task Rewrite_DoesNotChangeWhatIsHeld_OrLeaveAPartialFile()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await Hold(store, id, Bytes(1000));
        byte[] rewritten = Bytes(700, 5);

        await store.Rewrite(id, path => File.WriteAllBytes(path, rewritten));

        Assert.Equal([id], await store.GetHeldIDList());
        Assert.Empty(await store.GetPartialIDList());
        Assert.Equal(rewritten.Length, (await store.GetEstimate()).UsedBytes);
    }
}
