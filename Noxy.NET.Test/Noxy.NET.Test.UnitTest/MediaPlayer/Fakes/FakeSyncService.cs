using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeSyncService : ISyncService
{
    // Every call in the order it was made, as the method name.
    public List<string> CallList { get; } = [];

    public bool IsOffline { get; set; }

    // Thrown by SyncLibrary, after it is recorded, when set.
    public Exception? SyncLibraryException { get; set; }

    public event Action? Changed;

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public Task SyncLibrary(bool force = false)
    {
        CallList.Add(nameof(SyncLibrary));
        if (SyncLibraryException is not null) throw SyncLibraryException;

        return Task.CompletedTask;
    }

    public Task SyncPlaylistList(bool force = false)
    {
        CallList.Add(nameof(SyncPlaylistList));
        return Task.CompletedTask;
    }

    public Task SyncPlaylistDetail(Guid id)
    {
        CallList.Add(nameof(SyncPlaylistDetail));
        return Task.CompletedTask;
    }

    public Task ApplyMediaUpdated(ResponseMediaFile mediaFile)
    {
        CallList.Add(nameof(ApplyMediaUpdated));
        return Task.CompletedTask;
    }

    public Task ApplyMediaDeleted(Guid id)
    {
        CallList.Add(nameof(ApplyMediaDeleted));
        return Task.CompletedTask;
    }

    public Task ApplyPlaylistUpdated(ResponsePlaylist playlist)
    {
        CallList.Add(nameof(ApplyPlaylistUpdated));
        return Task.CompletedTask;
    }

    public Task ApplyPlaylistDeleted(Guid id)
    {
        CallList.Add(nameof(ApplyPlaylistDeleted));
        return Task.CompletedTask;
    }

    public Task ApplyUploadSucceeded(ResponseUploadResult result)
    {
        CallList.Add(nameof(ApplyUploadSucceeded));
        return Task.CompletedTask;
    }
}
