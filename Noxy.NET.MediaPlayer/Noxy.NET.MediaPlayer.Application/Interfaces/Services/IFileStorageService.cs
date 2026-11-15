using Noxy.NET.MediaPlayer.Application.Models.Upload;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IFileStorageService
{
    Stream OpenRead(string fileName);
    void Delete(string fileName);

    // Copies a stored file to a new file in the temporary folder and returns its path. Throws FileNotFoundException when the stored file is missing.
    string CopyToTemporary(string fileName);

    // The caller owns the stream. Disposing it deletes the temporary file.
    Stream OpenTemporary(string path);

    // Safe to repeat: a file that is already gone is left alone.
    void DeleteTemporary(string path);

    // Writes to the staging folder, where a file waits until its upload job has been processed.
    Task<StoredFile> StageAsync(Stream content, string extension, CancellationToken cancellationToken = default);

    // As above, but stops at maxBytes when that is below the configured maximum, throwing FileTooLargeException at the first byte over.
    Task<StoredFile> StageAsync(Stream content, string extension, long maxBytes, CancellationToken cancellationToken = default);

    // Moves a staged file into the library under the same name. Safe to repeat: a file that was already moved is left alone.
    void Promote(string fileName);

    void DeleteStaged(string fileName);

    IReadOnlyList<string> GetStagedFileNameList();
}
