using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeSyncService : ISyncService
{
    // Every call in the order it was made, as the method name.
    public List<string> CallList { get; } = [];

    public bool IsOffline { get; set; }

    public List<Guid> PlaylistDetailSyncList { get; } = [];

    public List<ResponsePlaylist> PlaylistUpdatedList { get; } = [];

    public List<Guid> PlaylistDeletedList { get; } = [];

    // Thrown by SyncLibrary, after it is recorded, when set.
    public Exception? SyncLibraryException { get; set; }

    public event Action? Changed;

    public event Action<LibraryDiff>? LibraryDiffApplied;

    public event Action<ResponseUploadResult>? UploadResultReceived;

    public void RaiseUploadResult(ResponseUploadResult result)
    {
        UploadResultReceived?.Invoke(result);
    }

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public void RaiseLibraryDiff(LibraryDiff diff)
    {
        LibraryDiffApplied?.Invoke(diff);
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
        PlaylistDetailSyncList.Add(id);
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
        PlaylistUpdatedList.Add(playlist);
        return Task.CompletedTask;
    }

    public Task ApplyPlaylistDeleted(Guid id)
    {
        CallList.Add(nameof(ApplyPlaylistDeleted));
        PlaylistDeletedList.Add(id);
        return Task.CompletedTask;
    }

    public Task ApplyUploadSucceeded(ResponseUploadResult result)
    {
        CallList.Add(nameof(ApplyUploadSucceeded));
        return Task.CompletedTask;
    }

    public Task ApplyUploadFailed(ResponseUploadResult result)
    {
        CallList.Add(nameof(ApplyUploadFailed));
        return Task.CompletedTask;
    }
}
