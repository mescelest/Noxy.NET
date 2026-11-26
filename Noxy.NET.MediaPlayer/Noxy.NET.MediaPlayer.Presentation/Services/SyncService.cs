using System.Net;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class SyncService(IApiClient clientApi, IMetadataStore storeMetadata) : ISyncService
{
    public event Action? Changed;

    public event Action<LibraryDiff>? LibraryDiffApplied;

    public event Action<ResponseUploadResult>? UploadResultReceived;

    public bool IsOffline { get; private set; }

    public async Task SyncLibrary(bool force = false)
    {
        List<ResponseMediaFile> local = await storeMetadata.GetMediaFileList();
        string? eTag = force || local.Count == 0 ? null : await storeMetadata.GetLibraryETag();

        ApiResponse<List<ResponseMediaFile>>? response = await Fetch(() => clientApi.GetLibrary(eTag));
        if (response?.Value is null) return;

        await storeMetadata.ReplaceMediaFileList(response.Value);
        await storeMetadata.SetLibraryETag(response.ETag);
        Changed?.Invoke();

        RaiseLibraryDiff(Diff(local, response.Value));
    }

    public async Task SyncPlaylistList(bool force = false)
    {
        string? eTag = force || (await storeMetadata.GetPlaylistList()).Count == 0 ? null : await storeMetadata.GetPlaylistListETag();

        ApiResponse<List<ResponsePlaylist>>? response = await Fetch(() => clientApi.GetPlaylistList(eTag));
        if (response?.Value is null) return;

        await storeMetadata.ReplacePlaylistList(response.Value);
        await storeMetadata.SetPlaylistListETag(response.ETag);
        Changed?.Invoke();

        await RefreshStaleDetailList(response.Value);
    }

    public async Task SyncPlaylistDetail(Guid id)
    {
        ApiResponse<ResponsePlaylistDetail>? response;
        try
        {
            response = await Fetch(() => clientApi.GetPlaylistDetail(id, null));
        }
        catch (ApiException exception) when (exception.Status == HttpStatusCode.NotFound)
        {
            await storeMetadata.DeletePlaylist(id);
            Changed?.Invoke();
            return;
        }

        if (response?.Value is null) return;

        await storeMetadata.SavePlaylistDetail(response.Value);
        Changed?.Invoke();
    }

    public async Task ApplyMediaUpdated(ResponseMediaFile mediaFile)
    {
        ResponseMediaFile? before = await storeMetadata.GetMediaFile(mediaFile.ID);

        await storeMetadata.SaveMediaFile(mediaFile);
        Changed?.Invoke();

        RaiseLibraryDiff(Diff(before is null ? [] : [before], [mediaFile]));
    }

    public async Task ApplyMediaDeleted(Guid id)
    {
        ResponseMediaFile? before = await storeMetadata.GetMediaFile(id);

        await storeMetadata.DeleteMediaFile(id);
        Changed?.Invoke();

        if (before is not null) RaiseLibraryDiff(new([], [id]));
    }

    public async Task ApplyPlaylistUpdated(ResponsePlaylist playlist)
    {
        ResponsePlaylist? local = (await storeMetadata.GetPlaylistList()).FirstOrDefault(x => x.ID == playlist.ID);
        if (local is not null && !IsNewer(playlist, local)) return;

        await storeMetadata.SavePlaylist(playlist);
        Changed?.Invoke();

        ResponsePlaylistDetail? detail = await storeMetadata.GetPlaylistDetail(playlist.ID);
        if (detail is not null && playlist.TimeModified > detail.TimeModified) await SyncPlaylistDetail(playlist.ID);
    }

    public async Task ApplyPlaylistDeleted(Guid id)
    {
        await storeMetadata.DeletePlaylist(id);
        Changed?.Invoke();
    }

    public async Task ApplyPlaylistOrder(List<ResponsePlaylist> playlistList)
    {
        await storeMetadata.ReplacePlaylistList(playlistList);
        Changed?.Invoke();
    }

    public async Task ApplyUploadSucceeded(ResponseUploadResult result)
    {
        if (result.Media is not null)
        {
            await ApplyMediaUpdated(result.Media);
        }

        UploadResultReceived?.Invoke(result);
    }

    public Task ApplyUploadFailed(ResponseUploadResult result)
    {
        UploadResultReceived?.Invoke(result);

        return Task.CompletedTask;
    }

    // Compares by ID: the bytes behind an ID never change, so a change that only touches metadata is not a change here.
    private static LibraryDiff Diff(List<ResponseMediaFile> before, List<ResponseMediaFile> after)
    {
        HashSet<Guid> beforeSet = [.. before.Select(x => x.ID)];
        HashSet<Guid> afterSet = [.. after.Select(x => x.ID)];

        List<ResponseMediaFile> addedList = after.Where(x => !beforeSet.Contains(x.ID)).ToList();
        List<Guid> removedIDList = before.Where(x => !afterSet.Contains(x.ID)).Select(x => x.ID).ToList();

        return new(addedList, removedIDList);
    }

    private void RaiseLibraryDiff(LibraryDiff diff)
    {
        if (!diff.IsEmpty) LibraryDiffApplied?.Invoke(diff);
    }

    private static bool IsNewer(ResponsePlaylist pushed, ResponsePlaylist local)
    {
        if (pushed.TimeModified != local.TimeModified) return pushed.TimeModified > local.TimeModified;

        return (pushed.TimeLastPlayed ?? DateTime.MinValue) > (local.TimeLastPlayed ?? DateTime.MinValue);
    }

    // Null when the server could not be reached or answered 304 (nothing to apply). Any other failure propagates.
    private async Task<ApiResponse<T>?> Fetch<T>(Func<Task<ApiResponse<T>>> call)
    {
        try
        {
            ApiResponse<T> response = await call();
            SetOffline(false);
            return response;
        }
        catch (Exception exception) when (exception.IsOfflineFailure)
        {
            SetOffline(true);
            return null;
        }
    }

    // A play does not change the entries, so only a later TimeModified makes a held detail stale.
    private async Task RefreshStaleDetailList(List<ResponsePlaylist> playlistList)
    {
        foreach (Guid id in await storeMetadata.GetHeldPlaylistIDList())
        {
            ResponsePlaylist? playlist = playlistList.FirstOrDefault(x => x.ID == id);
            ResponsePlaylistDetail? detail = await storeMetadata.GetPlaylistDetail(id);
            if (playlist is null || detail is null || playlist.TimeModified <= detail.TimeModified) continue;

            await SyncPlaylistDetail(id);
        }
    }

    private void SetOffline(bool offline)
    {
        if (IsOffline == offline) return;

        IsOffline = offline;
        Changed?.Invoke();
    }
}
