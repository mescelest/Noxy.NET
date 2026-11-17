using System.Text.Json;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Storage.Stores;

public class FileLocalQueueStore(string path) : ILocalQueueStore
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<LocalQueueSnapshot?> Get()
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(path)) return null;

            try
            {
                await using FileStream stream = File.OpenRead(path);
                return await JsonSerializer.DeserializeAsync<LocalQueueSnapshot>(stream, JsonSerializerOptions.Web);
            }
            catch (JsonException)
            {
                // A cache that was cut off or damaged is of no use: it goes, as if nothing was saved.
                File.Delete(path);
                return null;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    // Written to a file beside it first and then moved over it, so a crash in the middle leaves the old copy, never half of the new one.
    public async Task Save(LocalQueueSnapshot snapshot)
    {
        await _lock.WaitAsync();
        try
        {
            string? folder = Path.GetDirectoryName(Path.GetFullPath(path));
            if (folder is not null) Directory.CreateDirectory(folder);

            string temporaryPath = $"{path}.tmp";
            await using (FileStream stream = File.Create(temporaryPath)) await JsonSerializer.SerializeAsync(stream, snapshot, JsonSerializerOptions.Web);

            File.Move(temporaryPath, path, true);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task Clear()
    {
        await _lock.WaitAsync();
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        finally
        {
            _lock.Release();
        }
    }
}
