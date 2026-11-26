using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Storage.Extensions;

namespace Noxy.NET.MediaPlayer.Storage.Stores;

public class DiskFileStore(string baseFolder) : IFileStore
{
    private const string FilesFolderName = "files";
    private const string PartialFolderName = "partial";
    private const int BufferSize = 81920;
    private const string RewriteMarker = ".rewrite";

    private readonly string _filesFolder = Path.Combine(Path.GetFullPath(baseFolder), FilesFolderName);
    private readonly string _partialFolder = Path.Combine(Path.GetFullPath(baseFolder), PartialFolderName);

    public Task<string?> GetPath(Guid mediaFileID)
    {
        return Task.FromResult(FindHeldPathList(mediaFileID).FirstOrDefault());
    }

    public Task<Stream?> Open(Guid mediaFileID)
    {
        string? path = FindHeldPathList(mediaFileID).FirstOrDefault();

        // Delete sharing lets the player's file be removed while a reader is still closing.
        return Task.FromResult<Stream?>(path is null ? null : new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, BufferSize, true));
    }

    public Task Delete(Guid mediaFileID)
    {
        foreach (string path in FindHeldPathList(mediaFileID)) File.Delete(path);

        return Task.CompletedTask;
    }

    public Task<List<Guid>> GetHeldIDList()
    {
        return Task.FromResult(ListIDList(_filesFolder));
    }

    public Task<StorageEstimate> GetEstimate()
    {
        EnsureFolders();

        long used = new DirectoryInfo(_filesFolder).EnumerateFiles().Where(x => Guid.TryParse(Path.GetFileNameWithoutExtension(x.Name), out _)).Sum(x => x.Length);
        long free = new DriveInfo(Path.GetPathRoot(_filesFolder) ?? _filesFolder).AvailableFreeSpace;

        return Task.FromResult(new StorageEstimate(used, free));
    }

    public Task<long> GetPartialLength(Guid mediaFileID)
    {
        string path = PartialPath(mediaFileID);

        return Task.FromResult(File.Exists(path) ? new FileInfo(path).Length : 0);
    }

    public async Task AppendPartial(Guid mediaFileID, Stream content)
    {
        EnsureFolders();

        try
        {
            await using FileStream file = new(PartialPath(mediaFileID), FileMode.Append, FileAccess.Write, FileShare.None, BufferSize, true);
            await content.CopyToAsync(file, BufferSize);
        }
        catch (IOException exception) when (exception.IsDiskFull)
        {
            throw new StorageFullException(exception);
        }
    }

    public Task DiscardPartial(Guid mediaFileID)
    {
        File.Delete(PartialPath(mediaFileID));

        return Task.CompletedTask;
    }

    public Task<List<Guid>> GetPartialIDList()
    {
        return Task.FromResult(ListIDList(_partialFolder));
    }

    // A rename on one volume is atomic, so the held file is never half written: it is the old file, then the new one. A held file with another extension is removed afterwards.
    public Task Commit(Guid mediaFileID, string contentType)
    {
        string partialPath = PartialPath(mediaFileID);
        if (!File.Exists(partialPath)) throw new InvalidOperationException("There is no partial file.");

        EnsureFolders();
        string heldPath = Path.Combine(_filesFolder, $"{mediaFileID}{contentType.GetFileExtension()}");
        File.Move(partialPath, heldPath, true);

        foreach (string other in FindHeldPathList(mediaFileID).Where(x => !string.Equals(x, heldPath, StringComparison.Ordinal))) File.Delete(other);

        return Task.CompletedTask;
    }


    // The copy lives in the partial folder under a name that is not a GUID, so it is never listed. Moving it onto the held file is a rename on one volume, so the held file is the old file, then the new one.
    public Task Rewrite(Guid mediaFileID, Action<string> write)
    {
        string heldPath = FindHeldPathList(mediaFileID).FirstOrDefault() ?? throw new InvalidOperationException("The file is not held.");
        string copyPath = Path.Combine(_partialFolder, $"{mediaFileID}{RewriteMarker}{Path.GetExtension(heldPath)}");

        try
        {
            try
            {
                File.Copy(heldPath, copyPath, true);
            }
            catch (IOException exception) when (exception.IsDiskFull)
            {
                throw new StorageFullException(exception);
            }

            write(copyPath);
            File.Move(copyPath, heldPath, true);
        }
        finally
        {
            File.Delete(copyPath);
        }

        return Task.CompletedTask;
    }

    // The ID is a GUID, so a path built from it can never leave the base folder.
    private string PartialPath(Guid mediaFileID)
    {
        return Path.Combine(_partialFolder, mediaFileID.ToString());
    }

    private List<string> FindHeldPathList(Guid mediaFileID)
    {
        EnsureFolders();

        return Directory.EnumerateFiles(_filesFolder, $"{mediaFileID}.*").ToList();
    }

    // A name in the folder that is not a GUID is not ours and is left alone.
    private List<Guid> ListIDList(string folder)
    {
        EnsureFolders();

        List<Guid> idList = [];
        foreach (string path in Directory.EnumerateFiles(folder))
        {
            if (Guid.TryParse(Path.GetFileNameWithoutExtension(path), out Guid id)) idList.Add(id);
        }

        return idList;
    }

    private void EnsureFolders()
    {
        Directory.CreateDirectory(_filesFolder);
        Directory.CreateDirectory(_partialFolder);
    }
}
