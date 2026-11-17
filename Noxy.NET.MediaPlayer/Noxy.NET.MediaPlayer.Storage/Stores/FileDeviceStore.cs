using System.Text.Json;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Storage.Stores;

public class FileDeviceStore(string path) : IDeviceStore, IVolumeStore
{
    private sealed record DeviceSettings(Guid? DeviceID, string? DeviceName, int? Volume);

    private const int DefaultVolume = 100;

    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<Guid?> GetDeviceID()
    {
        return (await Read()).DeviceID;
    }

    public async Task SetDeviceID(Guid id)
    {
        await Update(x => x with { DeviceID = id });
    }

    public async Task<string?> GetDeviceName()
    {
        return (await Read()).DeviceName;
    }

    public async Task SetDeviceName(string name)
    {
        await Update(x => x with { DeviceName = name });
    }

    public async Task<int> GetVolume()
    {
        return (await Read()).Volume ?? DefaultVolume;
    }

    public async Task SetVolume(int volume)
    {
        await Update(x => x with { Volume = volume });
    }

    private async Task<DeviceSettings> Read()
    {
        await _lock.WaitAsync();
        try
        {
            return await ReadLocked();
        }
        finally
        {
            _lock.Release();
        }
    }

    // Reads the file, changes one value and writes it back through a file beside it, so a crash leaves the old settings and never half of the new ones.
    private async Task Update(Func<DeviceSettings, DeviceSettings> change)
    {
        await _lock.WaitAsync();
        try
        {
            DeviceSettings settings = change(await ReadLocked());

            string? folder = Path.GetDirectoryName(Path.GetFullPath(path));
            if (folder is not null) Directory.CreateDirectory(folder);

            string temporaryPath = $"{path}.tmp";
            await using (FileStream stream = File.Create(temporaryPath)) await JsonSerializer.SerializeAsync(stream, settings, JsonSerializerOptions.Web);

            File.Move(temporaryPath, path, true);
        }
        finally
        {
            _lock.Release();
        }
    }

    // A file that is missing or damaged holds nothing: a new device ID is made, the volume is full.
    private async Task<DeviceSettings> ReadLocked()
    {
        if (!File.Exists(path)) return new(null, null, null);

        try
        {
            await using FileStream stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<DeviceSettings>(stream, JsonSerializerOptions.Web) ?? new(null, null, null);
        }
        catch (JsonException)
        {
            File.Delete(path);
            return new(null, null, null);
        }
    }
}
