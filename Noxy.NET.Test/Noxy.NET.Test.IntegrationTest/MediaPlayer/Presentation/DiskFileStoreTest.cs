using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Storage.Stores;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class DiskFileStoreTest : FileStoreContract, IDisposable
{
    // A folder that does not exist yet, so the store has to create it.
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"file-store-{Guid.CreateVersion7()}");

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
    }

    protected override IFileStore CreateStore()
    {
        return new DiskFileStore(_folder);
    }

    private static async Task Hold(IFileStore store, Guid id, string contentType, byte[]? bytes = null)
    {
        await store.AppendPartial(id, new MemoryStream(bytes ?? [1, 2, 3]));
        await store.Commit(id, contentType);
    }

    [Theory]
    [InlineData("audio/mpeg", ".mp3")]
    [InlineData("audio/mp4", ".m4a")]
    [InlineData("audio/x-m4a", ".m4a")]
    [InlineData("audio/flac", ".flac")]
    [InlineData("audio/ogg", ".ogg")]
    [InlineData("audio/wav", ".wav")]
    [InlineData("audio/x-wav", ".wav")]
    [InlineData("audio/something-new", ".bin")]
    public async Task TheFileExtension_FollowsTheContentType_AndFallsBackToBin(string contentType, string extension)
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();

        await Hold(store, id, contentType);

        Assert.EndsWith(extension, await store.GetPath(id));
    }

    [Fact]
    public async Task TheHeldFile_LivesUnderFilesAndThePartialFileUnderPartial_InsideTheBaseFolder()
    {
        IFileStore store = CreateStore();
        Guid held = Guid.CreateVersion7();
        Guid partial = Guid.CreateVersion7();
        await Hold(store, held, "audio/mpeg");
        await store.AppendPartial(partial, new MemoryStream([1]));

        string? path = await store.GetPath(held);

        Assert.NotNull(path);
        Assert.Equal(Path.Combine(Path.GetFullPath(_folder), "files", $"{held}.mp3"), path);
        Assert.True(File.Exists(Path.Combine(Path.GetFullPath(_folder), "partial", partial.ToString())));
    }

    [Fact]
    public async Task Commit_LeavesNoTemporaryFilesBehind_AndReplacingAnExistingFileLeavesExactlyOne()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await Hold(store, id, "audio/mpeg", [1, 2, 3]);

        await Hold(store, id, "audio/mpeg", [9, 9, 9, 9]);

        string[] fileList = Directory.GetFiles(Path.Combine(_folder, "files"));
        Assert.Equal([Path.Combine(Path.GetFullPath(_folder), "files", $"{id}.mp3")], fileList);
        Assert.Empty(Directory.GetFiles(Path.Combine(_folder, "partial")));
    }

    [Fact]
    public async Task ANameInTheFolderThatIsNotAGuid_IsIgnoredByTheLists()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await Hold(store, id, "audio/mpeg");
        await File.WriteAllBytesAsync(Path.Combine(_folder, "files", "notes.txt"), [1], TestContext.Current.CancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(_folder, "partial", "stray"), [1], TestContext.Current.CancellationToken);

        Assert.Equal([id], await store.GetHeldIDList());
        Assert.Empty(await store.GetPartialIDList());
        Assert.Equal(3, (await store.GetEstimate()).UsedBytes);
    }

    [Fact]
    public async Task AMissingBaseFolder_IsCreated()
    {
        IFileStore store = CreateStore();
        Assert.False(Directory.Exists(_folder));

        Assert.Empty(await store.GetHeldIDList());

        Assert.True(Directory.Exists(Path.Combine(_folder, "files")));
        Assert.True(Directory.Exists(Path.Combine(_folder, "partial")));
    }

    [Fact]
    public async Task TheFreeBytes_AreTheVolumesAvailableFreeSpace()
    {
        IFileStore store = CreateStore();

        long free = (await store.GetEstimate()).FreeBytes;

        long expected = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(_folder))!).AvailableFreeSpace;
        Assert.InRange(free, expected - 50L * 1024 * 1024, expected + 50L * 1024 * 1024);
    }

    [Fact]
    public async Task APathBuiltFromAnID_IsAlwaysInsideTheBaseFolder()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await Hold(store, id, "audio/mpeg");

        string? path = await store.GetPath(id);

        Assert.StartsWith(Path.GetFullPath(_folder), path);
    }

    [Fact]
    public async Task AHeldFile_CanBeDeletedWhileAReaderIsStillOpen()
    {
        IFileStore store = CreateStore();
        Guid id = Guid.CreateVersion7();
        await Hold(store, id, "audio/mpeg");
        await using Stream? reader = await store.Open(id);

        await store.Delete(id);

        Assert.Null(await store.GetPath(id));
    }
}
