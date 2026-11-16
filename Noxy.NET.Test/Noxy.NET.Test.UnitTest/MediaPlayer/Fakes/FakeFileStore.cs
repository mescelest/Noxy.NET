using System.Collections.Concurrent;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeFileStore : IFileStore
{
    private const string PathPrefix = "/fake/files/";
    private const int ChunkSize = 4096;

    private readonly ConcurrentDictionary<Guid, (byte[] Bytes, string Extension)> _heldMap = [];
    private readonly ConcurrentDictionary<Guid, MemoryStream> _partialMap = [];

    // When set, AppendPartial throws StorageFullException and changes nothing.
    public bool IsFull { get; set; }

    // The free space GetEstimate reports.
    public long FreeBytes { get; set; } = 10L * 1024 * 1024 * 1024;

    // When set, AppendPartial throws StorageFullException once the partial file would pass this many bytes.
    public long? FullAfterPartialBytes { get; set; }

    // The held bytes of the file a path stands for, as a test sees it (a fake path is not a real file).
    public byte[] ReadPath(string path)
    {
        return _heldMap.Single(x => path == PathFor(x.Key, x.Value.Extension)).Value.Bytes;
    }

    // A test can put a file in the store without going through a download.
    public void Hold(Guid mediaFileID, byte[] bytes, string contentType = "audio/mpeg")
    {
        _heldMap[mediaFileID] = (bytes, contentType.GetFileExtension());
    }

    public Task<string?> GetPath(Guid mediaFileID)
    {
        return Task.FromResult(_heldMap.TryGetValue(mediaFileID, out (byte[] Bytes, string Extension) held) ? PathFor(mediaFileID, held.Extension) : null);
    }

    public Task<Stream?> Open(Guid mediaFileID)
    {
        return Task.FromResult<Stream?>(_heldMap.TryGetValue(mediaFileID, out (byte[] Bytes, string Extension) held) ? new MemoryStream(held.Bytes, false) : null);
    }

    public Task Delete(Guid mediaFileID)
    {
        _heldMap.TryRemove(mediaFileID, out _);

        return Task.CompletedTask;
    }

    public Task<List<Guid>> GetHeldIDList()
    {
        return Task.FromResult(_heldMap.Keys.ToList());
    }

    public Task<StorageEstimate> GetEstimate()
    {
        return Task.FromResult(new StorageEstimate(_heldMap.Values.Sum(x => (long)x.Bytes.Length), FreeBytes));
    }

    public Task<long> GetPartialLength(Guid mediaFileID)
    {
        return Task.FromResult(_partialMap.TryGetValue(mediaFileID, out MemoryStream? partial) ? partial.Length : 0);
    }

    public async Task AppendPartial(Guid mediaFileID, Stream content)
    {
        if (IsFull) throw new StorageFullException();

        if (!_partialMap.TryGetValue(mediaFileID, out MemoryStream? partial))
        {
            partial = new();
            _partialMap[mediaFileID] = partial;
        }

        // Chunk by chunk like a real file: what was read before a failure stays in the partial file.
        byte[] buffer = new byte[ChunkSize];
        int read;
        while ((read = await content.ReadAsync(buffer)) > 0)
        {
            if (FullAfterPartialBytes is not null && partial.Length + read > FullAfterPartialBytes) throw new StorageFullException();

            partial.Position = partial.Length;
            partial.Write(buffer, 0, read);
        }
    }

    public Task<Stream> OpenPartial(Guid mediaFileID)
    {
        if (!_partialMap.TryGetValue(mediaFileID, out MemoryStream? partial)) throw new InvalidOperationException("There is no partial file.");

        return Task.FromResult<Stream>(new MemoryStream(partial.ToArray(), false));
    }

    public Task DiscardPartial(Guid mediaFileID)
    {
        _partialMap.TryRemove(mediaFileID, out _);

        return Task.CompletedTask;
    }

    public Task<List<Guid>> GetPartialIDList()
    {
        return Task.FromResult(_partialMap.Keys.ToList());
    }

    public Task Commit(Guid mediaFileID, string contentType)
    {
        if (!_partialMap.TryRemove(mediaFileID, out MemoryStream? partial)) throw new InvalidOperationException("There is no partial file.");

        _heldMap[mediaFileID] = (partial.ToArray(), contentType.GetFileExtension());

        return Task.CompletedTask;
    }

    private static string PathFor(Guid mediaFileID, string extension)
    {
        return $"{PathPrefix}{mediaFileID}{extension}";
    }
}
