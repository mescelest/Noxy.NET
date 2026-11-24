using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Storage.Stores;

public class DiskArtCacheStore(string baseFolder) : IArtCacheStore
{
    private const string ArtFolderName = "art";
    private const string PictureExtension = ".jpg";
    private const string TemporaryExtension = ".tmp";
    private const char VersionSeparator = '.';

    private readonly string _artFolder = Path.Combine(Path.GetFullPath(baseFolder), ArtFolderName);

    public async Task<byte[]?> Get(Guid albumID, DateTime version)
    {
        string path = PathOf(albumID, version);

        return File.Exists(path) ? await File.ReadAllBytesAsync(path) : null;
    }

    // The version is in the file name, so a copy of another version is never mistaken for this one. Written to a temporary name and moved, so a copy is never half written.
    public async Task Save(Guid albumID, DateTime version, byte[] bytes)
    {
        Directory.CreateDirectory(_artFolder);
        string path = PathOf(albumID, version);
        string temporary = Path.Combine(_artFolder, $"{Guid.CreateVersion7()}{TemporaryExtension}");

        try
        {
            await File.WriteAllBytesAsync(temporary, bytes);
            File.Move(temporary, path, true);
        }
        catch
        {
            File.Delete(temporary);
            throw;
        }

        foreach (string other in FindPathList(albumID).Where(x => !string.Equals(x, path, StringComparison.Ordinal))) File.Delete(other);
    }

    public Task Delete(Guid albumID)
    {
        foreach (string path in FindPathList(albumID)) File.Delete(path);

        return Task.CompletedTask;
    }

    public Task Clear()
    {
        if (!Directory.Exists(_artFolder)) return Task.CompletedTask;

        // Only what this store wrote: a file in the folder that is not a picture or its temporary copy is not ours.
        foreach (string path in Directory.EnumerateFiles(_artFolder).Where(x => x.EndsWith(PictureExtension, StringComparison.Ordinal) || x.EndsWith(TemporaryExtension, StringComparison.Ordinal))) File.Delete(path);

        return Task.CompletedTask;
    }

    // The ID is a GUID and the version a number, so a path built from them can never leave the folder.
    private string PathOf(Guid albumID, DateTime version)
    {
        return Path.Combine(_artFolder, $"{albumID}{VersionSeparator}{version.ToUniversalTime().Ticks}{PictureExtension}");
    }

    private List<string> FindPathList(Guid albumID)
    {
        if (!Directory.Exists(_artFolder)) return [];

        return Directory.EnumerateFiles(_artFolder, $"{albumID}{VersionSeparator}*{PictureExtension}").ToList();
    }
}
