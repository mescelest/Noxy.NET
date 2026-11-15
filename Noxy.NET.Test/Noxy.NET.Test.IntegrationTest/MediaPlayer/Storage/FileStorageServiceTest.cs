using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Models.Upload;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Persistence.Services;

namespace Noxy.NET.Test.MediaPlayer.Storage;

public class FileStorageServiceTest : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    private FileStorageService Create(long maxSizeBytes = 1024)
    {
        return new(Options.Create(new ConfigStorage { RootPath = _root, MaxSizeBytes = maxSizeBytes, AllowedExtensionList = ["mp3"] }));
    }

    private static MemoryStream Bytes(int count)
    {
        return new(new byte[count]);
    }

    private string StagingPath(string fileName)
    {
        return Path.Combine(_root, FileStorageService.StagingFolderName, fileName);
    }

    // Files reach the library by being staged and then promoted.
    private static async Task<string> AddToLibrary(FileStorageService service, byte[] content)
    {
        StoredFile stored = await service.StageAsync(new MemoryStream(content), "mp3", TestContext.Current.CancellationToken);
        service.Promote(stored.FileName);
        return stored.FileName;
    }

    [Fact]
    public void Constructor_CreatesMissingRootAndStagingDirectories()
    {
        Create();

        Assert.True(Directory.Exists(_root));
        Assert.True(Directory.Exists(Path.Combine(_root, FileStorageService.StagingFolderName)));
    }

    [Fact]
    public async Task Stage_StoresContentUnderGeneratedNameWithExtension()
    {
        FileStorageService service = Create();

        string fileName = (await service.StageAsync(new MemoryStream([1, 2, 3]), ".MP3", TestContext.Current.CancellationToken)).FileName;

        Assert.EndsWith(".mp3", fileName);
        Assert.True(Guid.TryParse(Path.GetFileNameWithoutExtension(fileName), out _));
        Assert.Equal([1, 2, 3], await File.ReadAllBytesAsync(StagingPath(fileName), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Stage_WritesUnderTheStagingFolderNotTheLibrary()
    {
        FileStorageService service = Create();

        StoredFile stored = await service.StageAsync(new MemoryStream([1, 2, 3]), "mp3", TestContext.Current.CancellationToken);

        Assert.True(File.Exists(StagingPath(stored.FileName)));
        Assert.False(File.Exists(Path.Combine(_root, stored.FileName)));
    }

    [Fact]
    public async Task Stage_ReportsSizeAndSha256()
    {
        byte[] content = [1, 2, 3, 4, 5];

        StoredFile stored = await Create().StageAsync(new MemoryStream(content), "mp3", TestContext.Current.CancellationToken);

        Assert.Equal(5, stored.SizeBytes);
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(content)), stored.ContentHash);
    }

    [Fact]
    public async Task Stage_HashMatchesTheFileOnDisk()
    {
        byte[] content = new byte[300_000];
        Random.Shared.NextBytes(content);

        StoredFile stored = await Create(1_000_000).StageAsync(new MemoryStream(content), "mp3", TestContext.Current.CancellationToken);

        byte[] onDisk = await File.ReadAllBytesAsync(StagingPath(stored.FileName), TestContext.Current.CancellationToken);
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(onDisk)), stored.ContentHash);
    }

    [Fact]
    public async Task Stage_TwiceGivesDifferentNames()
    {
        FileStorageService service = Create();

        string first = (await service.StageAsync(Bytes(1), "mp3", TestContext.Current.CancellationToken)).FileName;
        string second = (await service.StageAsync(Bytes(1), "mp3", TestContext.Current.CancellationToken)).FileName;

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task Stage_ExactlyMaxSize_Succeeds()
    {
        FileStorageService service = Create(100);

        string fileName = (await service.StageAsync(Bytes(100), "mp3", TestContext.Current.CancellationToken)).FileName;

        Assert.True(File.Exists(StagingPath(fileName)));
    }

    [Fact]
    public async Task Stage_OverMaxSize_ThrowsAndLeavesNothingStaged()
    {
        FileStorageService service = Create(100);

        await Assert.ThrowsAsync<FileTooLargeException>(() => service.StageAsync(Bytes(101), "mp3", TestContext.Current.CancellationToken));

        Assert.Empty(service.GetStagedFileNameList());
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("mp3/../x")]
    [InlineData("m p3")]
    public async Task Stage_InvalidExtension_Throws(string extension)
    {
        FileStorageService service = Create();

        await Assert.ThrowsAsync<ArgumentException>(() => service.StageAsync(Bytes(1), extension, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Stage_WithATighterLimit_ThrowsAndLeavesNothingStaged()
    {
        FileStorageService service = Create(1000);

        await Assert.ThrowsAsync<FileTooLargeException>(() => service.StageAsync(Bytes(51), "mp3", 50, TestContext.Current.CancellationToken));

        Assert.Empty(service.GetStagedFileNameList());
    }

    [Fact]
    public async Task Stage_WithATighterLimit_AcceptsAFileExactlyAtIt()
    {
        StoredFile stored = await Create(1000).StageAsync(Bytes(50), "mp3", 50, TestContext.Current.CancellationToken);

        Assert.Equal(50, stored.SizeBytes);
    }

    [Fact]
    public async Task Stage_WithALooserLimit_StillUsesTheConfiguredMaximum()
    {
        FileStorageService service = Create(100);

        await Assert.ThrowsAsync<FileTooLargeException>(() => service.StageAsync(Bytes(101), "mp3", 5000, TestContext.Current.CancellationToken));

        Assert.Empty(service.GetStagedFileNameList());
    }

    [Fact]
    public async Task Promote_MovesTheFileIntoTheLibraryUnderTheSameName()
    {
        FileStorageService service = Create();
        StoredFile stored = await service.StageAsync(new MemoryStream([9, 8, 7]), "mp3", TestContext.Current.CancellationToken);

        service.Promote(stored.FileName);

        await using Stream stream = service.OpenRead(stored.FileName);
        using MemoryStream copy = new();
        await stream.CopyToAsync(copy, TestContext.Current.CancellationToken);
        Assert.Equal([9, 8, 7], copy.ToArray());
        Assert.Empty(service.GetStagedFileNameList());
    }

    [Fact]
    public async Task Promote_CalledTwice_IsANoOpTheSecondTime()
    {
        FileStorageService service = Create();
        StoredFile stored = await service.StageAsync(Bytes(3), "mp3", TestContext.Current.CancellationToken);

        service.Promote(stored.FileName);
        service.Promote(stored.FileName);

        Assert.True(File.Exists(Path.Combine(_root, stored.FileName)));
    }

    [Fact]
    public void Promote_NeitherPlaceHasTheFile_ThrowsFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(() => Create().Promote("missing.mp3"));
    }

    [Fact]
    public async Task Delete_RemovesTheLibraryFile()
    {
        FileStorageService service = Create();
        string fileName = await AddToLibrary(service, [1]);

        service.Delete(fileName);

        Assert.False(File.Exists(Path.Combine(_root, fileName)));
    }

    [Fact]
    public void Delete_MissingFile_DoesNotThrow()
    {
        Create().Delete("missing.mp3");
    }

    [Fact]
    public async Task DeleteStaged_RemovesTheStagedFile()
    {
        FileStorageService service = Create();
        StoredFile stored = await service.StageAsync(Bytes(3), "mp3", TestContext.Current.CancellationToken);

        service.DeleteStaged(stored.FileName);

        Assert.False(File.Exists(StagingPath(stored.FileName)));
    }

    [Fact]
    public void DeleteStaged_MissingFile_DoesNotThrow()
    {
        Create().DeleteStaged("missing.mp3");
    }

    [Fact]
    public async Task GetStagedFileNameList_ReturnsOnlyStagedNames()
    {
        FileStorageService service = Create();
        StoredFile first = await service.StageAsync(Bytes(3), "mp3", TestContext.Current.CancellationToken);
        StoredFile second = await service.StageAsync(Bytes(3), "mp3", TestContext.Current.CancellationToken);
        await AddToLibrary(service, [1]);

        Assert.Equal(new[] { first.FileName, second.FileName }.Order(), service.GetStagedFileNameList().Order());
    }

    [Theory]
    [InlineData("../secret.mp3")]
    [InlineData("sub/file.mp3")]
    [InlineData("")]
    public void EveryNameTakingMethod_PathInName_Throws(string fileName)
    {
        FileStorageService service = Create();

        Assert.Throws<ArgumentException>(() => service.OpenRead(fileName));
        Assert.Throws<ArgumentException>(() => service.Delete(fileName));
        Assert.Throws<ArgumentException>(() => service.Promote(fileName));
        Assert.Throws<ArgumentException>(() => service.DeleteStaged(fileName));
    }

    private string TemporaryFolder => Path.Combine(_root, FileStorageService.TemporaryFolderName);

    [Fact]
    public void Constructor_RemovesLeftoverTemporaryFiles()
    {
        Directory.CreateDirectory(TemporaryFolder);
        string leftover = Path.Combine(TemporaryFolder, "left-behind.mp3");
        File.WriteAllBytes(leftover, [1]);

        Create();

        Assert.False(File.Exists(leftover));
    }

    [Fact]
    public void Constructor_LeftoverThatCannotBeDeleted_StillConstructs()
    {
        Directory.CreateDirectory(TemporaryFolder);
        using FileStream held = new(Path.Combine(TemporaryFolder, "held.mp3"), FileMode.Create, FileAccess.Write, FileShare.None);

        FileStorageService service = Create();

        Assert.NotNull(service);
    }

    [Fact]
    public async Task CopyToTemporary_TwoCopiesOfOneFile_GetDifferentPathsInTheTemporaryFolder()
    {
        FileStorageService service = Create();
        string fileName = await AddToLibrary(service, [1, 2, 3]);

        string first = service.CopyToTemporary(fileName);
        string second = service.CopyToTemporary(fileName);

        Assert.NotEqual(first, second);
        Assert.Equal(TemporaryFolder, Path.GetDirectoryName(first));
        Assert.Equal([1, 2, 3], await File.ReadAllBytesAsync(first, TestContext.Current.CancellationToken));
        Assert.Equal([1, 2, 3], await File.ReadAllBytesAsync(Path.Combine(_root, fileName), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task OpenTemporary_DisposingTheStream_RemovesTheCopy_AndLeavesTheStoredFile()
    {
        FileStorageService service = Create();
        string fileName = await AddToLibrary(service, [1, 2, 3]);
        string copy = service.CopyToTemporary(fileName);

        await using (Stream _ = service.OpenTemporary(copy))
        {
            Assert.True(File.Exists(copy));
        }

        Assert.False(File.Exists(copy));
        Assert.True(File.Exists(Path.Combine(_root, fileName)));
    }

    [Fact]
    public async Task DeleteTemporary_RemovesTheCopy_AndRepeatingIsSafe()
    {
        FileStorageService service = Create();
        string copy = service.CopyToTemporary(await AddToLibrary(service, [1]));

        service.DeleteTemporary(copy);
        service.DeleteTemporary(copy);

        Assert.False(File.Exists(copy));
    }

    [Fact]
    public async Task CopyToTemporary_MissingFile_ThrowsFileNotFound()
    {
        FileStorageService service = Create();
        string fileName = await AddToLibrary(service, [1]);
        File.Delete(Path.Combine(_root, fileName));

        Assert.Throws<FileNotFoundException>(() => service.CopyToTemporary(fileName));
    }

    [Theory]
    [InlineData("temporary/../outside.mp3")]
    [InlineData("outside.mp3")]
    [InlineData("staging/inside-staging.mp3")]
    [InlineData("temporary/sub/nested.mp3")]
    public void OpenTemporaryAndDeleteTemporary_PathOutsideTheTemporaryFolder_AreRejected(string relative)
    {
        FileStorageService service = Create();
        string path = Path.Combine(_root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? throw new InvalidOperationException("The path has no directory."));
        File.WriteAllBytes(Path.GetFullPath(path), [1]);

        Assert.Throws<ArgumentException>(() => service.OpenTemporary(path));
        Assert.Throws<ArgumentException>(() => service.DeleteTemporary(path));
        Assert.True(File.Exists(Path.GetFullPath(path)));
    }
}
