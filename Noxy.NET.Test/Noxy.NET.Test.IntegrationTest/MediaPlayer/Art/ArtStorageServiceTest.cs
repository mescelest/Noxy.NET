using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Persistence.Services;

namespace Noxy.NET.Test.MediaPlayer.Art;

public class ArtStorageServiceTest : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly ConfigStorage _config;

    public ArtStorageServiceTest()
    {
        _config = new() { RootPath = _root, AllowedExtensionList = ["mp3"] };
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    private ArtStorageService Create()
    {
        return new(Options.Create(_config));
    }

    private string AlbumFolder()
    {
        return Path.Combine(_root, _config.AlbumArtFolderName);
    }

    private static byte[] ReadAll(Stream? stream)
    {
        Assert.NotNull(stream);
        using (stream)
        {
            using MemoryStream copy = new();
            stream.CopyTo(copy);
            return copy.ToArray();
        }
    }

    [Fact]
    public async Task SaveAlbumArt_ThenOpen_ReturnsTheSameBytes()
    {
        ArtStorageService service = Create();
        Guid albumID = Guid.NewGuid();
        byte[] jpeg = [0xFF, 0xD8, 1, 2, 3];

        await service.SaveAlbumArt(albumID, jpeg, TestContext.Current.CancellationToken);

        Assert.Equal(jpeg, ReadAll(service.OpenAlbumArt(albumID)));
    }

    [Fact]
    public async Task SaveAlbumArt_Again_ReplacesTheBytes()
    {
        ArtStorageService service = Create();
        Guid albumID = Guid.NewGuid();
        await service.SaveAlbumArt(albumID, [1, 2, 3], TestContext.Current.CancellationToken);

        await service.SaveAlbumArt(albumID, [9, 8], TestContext.Current.CancellationToken);

        Assert.Equal([9, 8], ReadAll(service.OpenAlbumArt(albumID)));
    }

    [Fact]
    public void OpenAlbumArt_OfAnUnknownAlbum_ReturnsNull()
    {
        Assert.Null(Create().OpenAlbumArt(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAlbumArt_RemovesTheFile_AndIsHarmlessTwice()
    {
        ArtStorageService service = Create();
        Guid albumID = Guid.NewGuid();
        await service.SaveAlbumArt(albumID, [1], TestContext.Current.CancellationToken);

        service.DeleteAlbumArt(albumID);
        service.DeleteAlbumArt(albumID);

        Assert.Null(service.OpenAlbumArt(albumID));
        Assert.Empty(Directory.GetFiles(AlbumFolder()));
    }

    [Fact]
    public void DeleteAlbumArt_OfAnUnknownAlbum_IsHarmless()
    {
        Create().DeleteAlbumArt(Guid.NewGuid());
    }

    [Fact]
    public async Task SaveAlbumArt_LeavesOnlyTheFinalFile_NoTemporaryOne()
    {
        ArtStorageService service = Create();
        Guid albumID = Guid.NewGuid();

        await service.SaveAlbumArt(albumID, [1, 2], TestContext.Current.CancellationToken);
        await service.SaveAlbumArt(albumID, [3, 4], TestContext.Current.CancellationToken);

        string[] files = Directory.GetFiles(AlbumFolder());
        Assert.Single(files);
        Assert.Equal($"{albumID}.jpg", Path.GetFileName(files[0]));
    }

    [Fact]
    public async Task OpenAlbumArt_NeverReturnsAHalfWrittenFile_LeftByAnInterruptedSave()
    {
        ArtStorageService service = Create();
        Guid albumID = Guid.NewGuid();
        File.WriteAllBytes(Path.Combine(AlbumFolder(), $"{albumID}.jpg.tmp"), [1]);

        Assert.Null(service.OpenAlbumArt(albumID));

        await service.SaveAlbumArt(albumID, [5, 6], TestContext.Current.CancellationToken);
        Assert.Equal([5, 6], ReadAll(service.OpenAlbumArt(albumID)));
    }

    [Fact]
    public async Task SaveAlbumArt_WhileAReaderHasTheOldFileOpen_StillSaves()
    {
        ArtStorageService service = Create();
        Guid albumID = Guid.NewGuid();
        await service.SaveAlbumArt(albumID, [1, 2], TestContext.Current.CancellationToken);

        using Stream? reader = service.OpenAlbumArt(albumID);
        await service.SaveAlbumArt(albumID, [3, 4], TestContext.Current.CancellationToken);

        Assert.Equal([3, 4], ReadAll(service.OpenAlbumArt(albumID)));
    }

    [Fact]
    public async Task ConfiguredFolderName_IsTheOneThatIsUsed()
    {
        _config.AlbumArtFolderName = "covers";
        ArtStorageService service = Create();
        Guid albumID = Guid.CreateVersion7();

        await service.SaveAlbumArt(albumID, [1, 2], TestContext.Current.CancellationToken);

        Assert.True(File.Exists(Path.Combine(_root, "covers", $"{albumID}.jpg")));
        Assert.False(Directory.Exists(Path.Combine(_root, "albumart")));
    }
}
