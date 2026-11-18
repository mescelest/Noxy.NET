using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class PlaylistService(IApiClient clientApi, ISyncService serviceSync, IDownloadService serviceDownload, IMetadataStore storeMetadata, IPlaybackControlService serviceControl) : IPlaylistService
{
    public async Task<ResponsePlaylist> Create(string name)
    {
        ResponsePlaylist playlist = await clientApi.CreatePlaylist(new() { Name = name.Trim() });
        await serviceSync.ApplyPlaylistUpdated(playlist);

        return playlist;
    }

    public async Task Open(Guid id)
    {
        await serviceSync.SyncPlaylistDetail(id);
        await DownloadMissing(id);
    }

    public async Task Refresh(Guid id)
    {
        await serviceSync.SyncPlaylistDetail(id);
    }

    public async Task DownloadAll(Guid id)
    {
        await DownloadMissing(id);
    }

    public async Task Play(Guid id, Guid? startEntryID = null)
    {
        await serviceControl.StartPlaylist(id, startEntryID);
    }

    public async Task Rename(Guid id, string name)
    {
        await Update(id, new() { Name = new(name.Trim()) });
    }

    public async Task SetAllowDuplicates(Guid id, bool allow)
    {
        await Update(id, new() { AllowDuplicates = new(allow) });
    }

    public async Task SetShuffle(Guid id, ShuffleModeEnum? mode)
    {
        await Update(id, new() { Shuffle = new(mode) });
    }

    public async Task SetRepeat(Guid id, RepeatModeEnum? mode)
    {
        await Update(id, new() { Repeat = new(mode) });
    }

    public async Task RemoveEntryList(Guid id, List<Guid> entryIDList)
    {
        if (entryIDList.Count == 0) return;

        await Save(await clientApi.RemovePlaylistEntryList(id, new() { EntryIDList = [.. entryIDList] }));
    }

    public async Task MoveEntry(Guid id, Guid entryID, int newPosition)
    {
        await Save(await clientApi.MovePlaylistEntry(id, entryID, new() { Position = newPosition }));
    }

    public async Task Delete(Guid id)
    {
        await clientApi.DeletePlaylist(id);
        await serviceSync.ApplyPlaylistDeleted(id);
    }

    // The answer of an update is the summary; a held detail is downloaded again by the sync when the summary is newer.
    private async Task Update(Guid id, RequestUpdatePlaylist request)
    {
        await serviceSync.ApplyPlaylistUpdated(await clientApi.UpdatePlaylist(id, request));
    }

    // The detail first, so the summary that follows finds it as new as itself and does not fetch it again.
    private async Task Save(ResponsePlaylistDetail detail)
    {
        await storeMetadata.SavePlaylistDetail(detail);
        await serviceSync.ApplyPlaylistUpdated(detail);
    }

    private async Task DownloadMissing(Guid id)
    {
        ResponsePlaylistDetail? detail = await storeMetadata.GetPlaylistDetail(id);
        if (detail is null) return;

        HashSet<Guid> onDeviceSet = [.. (await storeMetadata.GetFileStateList()).Where(x => x.State == FileSyncStateEnum.OnDevice).Select(x => x.MediaFileID)];
        List<Guid> missingList = [.. detail.EntryList.Select(x => x.MediaFileID).Distinct().Where(x => !onDeviceSet.Contains(x))];
        if (missingList.Count > 0) await serviceDownload.QueueMissing(missingList);
    }
}
