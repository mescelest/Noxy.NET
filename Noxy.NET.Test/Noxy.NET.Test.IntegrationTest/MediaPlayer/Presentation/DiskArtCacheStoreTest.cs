using Noxy.NET.MediaPlayer.Storage;
using Noxy.NET.MediaPlayer.Storage.Stores;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class DiskArtCacheStoreTest : IDisposable
{
    private static readonly DateTime Version = new(2026, 11, 24, 10, 0, 0, DateTimeKind.Utc);

    // A folder that does not exist yet, so the store has to create it.
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"art-cache-{Guid.CreateVersion7()}");

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
    }

    private DiskArtCacheStore Create()
    {
        return new(_folder);
    }

    [Fact]
    public async Task Get_WithNoCopy_ReturnsNull()
    {
        Assert.Null(await Create().Get(Guid.CreateVersion7(), Version));
    }

    [Fact]
    public async Task Save_ThenGet_ReturnsTheBytes_AlsoFromAFreshStore()
    {
        Guid id = Guid.CreateVersion7();
        await Create().Save(id, Version, [1, 2, 3]);

        Assert.Equal<byte[]?>([1, 2, 3], await Create().Get(id, Version));
    }

    [Fact]
    public async Task Get_OfAnotherVersion_ReturnsNull_AndKeepsTheCopy()
    {
        DiskArtCacheStore store = Create();
        Guid id = Guid.CreateVersion7();
        await store.Save(id, Version, [1]);

        Assert.Null(await store.Get(id, Version.AddSeconds(1)));
        Assert.Equal<byte[]?>([1], await store.Get(id, Version));
    }

    [Fact]
    public async Task Save_OfANewVersion_ReplacesTheOldCopy_SoOnlyOneFileIsHeld()
    {
        DiskArtCacheStore store = Create();
        Guid id = Guid.CreateVersion7();
        await store.Save(id, Version, [1]);

        await store.Save(id, Version.AddHours(1), [2]);

        Assert.Null(await store.Get(id, Version));
        Assert.Equal<byte[]?>([2], await store.Get(id, Version.AddHours(1)));
        Assert.Single(Directory.GetFiles(_folder, "*", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task Save_OfTheSameVersion_ReplacesTheBytes()
    {
        DiskArtCacheStore store = Create();
        Guid id = Guid.CreateVersion7();
        await store.Save(id, Version, [1]);

        await store.Save(id, Version, [7, 7]);

        Assert.Equal<byte[]?>([7, 7], await store.Get(id, Version));
    }

    [Fact]
    public async Task Delete_RemovesTheCopy_AndIsHarmlessForAnAlbumWithNone()
    {
        DiskArtCacheStore store = Create();
        Guid id = Guid.CreateVersion7();
        Guid other = Guid.CreateVersion7();
        await store.Save(id, Version, [1]);
        await store.Save(other, Version, [2]);

        await store.Delete(id);
        await store.Delete(Guid.CreateVersion7());

        Assert.Null(await store.Get(id, Version));
        Assert.Equal<byte[]?>([2], await store.Get(other, Version));
    }

    [Fact]
    public async Task Get_LeavesAFileThatIsNotOurs_Alone()
    {
        DiskArtCacheStore store = Create();
        Guid id = Guid.CreateVersion7();
        await store.Save(id, Version, [1]);
        string folder = Directory.GetDirectories(_folder).Single();
        File.WriteAllBytes(Path.Combine(folder, "notes.txt"), [9]);

        await store.Delete(id);

        Assert.True(File.Exists(Path.Combine(folder, "notes.txt")));
    }

    [Fact]
    public async Task Clear_RemovesEveryCopy_AndIsHarmlessWhenThereAreNone()
    {
        DiskArtCacheStore store = Create();
        await store.Clear();
        Guid first = Guid.CreateVersion7();
        Guid second = Guid.CreateVersion7();
        await store.Save(first, Version, [1]);
        await store.Save(second, Version, [2]);

        await store.Clear();

        Assert.Null(await store.Get(first, Version));
        Assert.Null(await store.Get(second, Version));
    }

    [Fact]
    public async Task TheFolderName_IsTheOneGiven_ElseTheDefault()
    {
        Guid id = Guid.CreateVersion7();

        await new DiskArtCacheStore(_folder, "covers").Save(id, Version, [1]);
        await new DiskArtCacheStore(_folder).Save(id, Version, [2]);

        Assert.True(Directory.Exists(Path.Combine(_folder, "covers")));
        Assert.True(Directory.Exists(Path.Combine(_folder, StorageConstants.ArtCacheFolderName)));
        Assert.Equal<byte[]?>([1], await new DiskArtCacheStore(_folder, "covers").Get(id, Version));
        Assert.Equal<byte[]?>([2], await new DiskArtCacheStore(_folder).Get(id, Version));
    }
}
