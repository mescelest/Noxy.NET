using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

// One file per media file on the device. A file is held only after it was committed from the partial area, which is where every download is built. A disk that is full throws StorageFullException and leaves the held files untouched.
public interface IFileStore
{
    // The full path of a held file, null when not held. A partial file has no path.
    Task<string?> GetPath(Guid mediaFileID);

    // Null when not held. The caller disposes the stream.
    Task<Stream?> Open(Guid mediaFileID);

    // An ID that is not held is harmless.
    Task Delete(Guid mediaFileID);

    Task<List<Guid>> GetHeldIDList();

    Task<StorageEstimate> GetEstimate();

    // 0 when there is none.
    Task<long> GetPartialLength(Guid mediaFileID);

    // Adds the bytes to the end of the partial file, creating it when there is none.
    Task AppendPartial(Guid mediaFileID, Stream content);

    // An ID with no partial file is harmless.
    Task DiscardPartial(Guid mediaFileID);

    Task<List<Guid>> GetPartialIDList();

    // Moves the partial file into the store, replacing a held file, and removes the partial. Throws InvalidOperationException when there is no partial file. The content type decides the file's extension.
    Task Commit(Guid mediaFileID, string contentType);

    // Lets write change a copy of the held file and swaps the copy in only when write returned: a failure of write or of the swap leaves the held file as it was and removes the copy, and is rethrown. write gets the path of the copy, which has the held file's extension and is not part of the held or partial lists. Throws InvalidOperationException when the file is not held, and StorageFullException when the copy does not fit.
    Task Rewrite(Guid mediaFileID, Action<string> write);
}
