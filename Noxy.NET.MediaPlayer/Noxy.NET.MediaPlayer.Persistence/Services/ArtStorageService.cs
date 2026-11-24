using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Persistence.Services;

public class ArtStorageService : IArtStorageService
{
    public const string ArtFolderName = "art";
    public const string AlbumFolderName = "album";

    private const string PictureExtension = ".jpg";
    private const string TemporaryExtension = ".tmp";

    private readonly string _albumFolder;

    public ArtStorageService(IOptions<ConfigStorage> options)
    {
        _albumFolder = Path.Combine(Path.GetFullPath(options.Value.RootPath), ArtFolderName, AlbumFolderName);
        Directory.CreateDirectory(_albumFolder);
    }

    public async Task SaveAlbumArt(Guid albumID, byte[] jpeg, CancellationToken cancellationToken = default)
    {
        string path = PathOf(albumID);
        string temporary = Path.Combine(_albumFolder, $"{Guid.CreateVersion7()}{TemporaryExtension}");

        try
        {
            await File.WriteAllBytesAsync(temporary, jpeg, cancellationToken);
            File.Move(temporary, path, true);
        }
        catch
        {
            File.Delete(temporary);
            throw;
        }
    }

    // The picture is small, so it is read whole and the file closed at once: Windows will not replace a file a reader still holds open.
    public Stream? OpenAlbumArt(Guid albumID)
    {
        try
        {
            return new MemoryStream(File.ReadAllBytes(PathOf(albumID)));
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return null;
        }
    }

    public void DeleteAlbumArt(Guid albumID)
    {
        File.Delete(PathOf(albumID));
    }

    private string PathOf(Guid albumID)
    {
        return Path.Combine(_albumFolder, $"{albumID}{PictureExtension}");
    }
}
