using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class FileReconciler(IFileStore storeFile, IMetadataStore storeMetadata, IDownloadService serviceDownload) : IFileReconciler
{
    public async Task Apply(LibraryDiff diff)
    {
        foreach (Guid id in diff.RemovedIDList) await Forget(id);
    }

    public async Task Repair()
    {
        foreach (Guid id in await storeFile.GetPartialIDList()) await storeFile.DiscardPartial(id);

        Dictionary<Guid, ResponseMediaFile> mediaMap = (await storeMetadata.GetMediaFileList()).ToDictionary(x => x.ID);
        Dictionary<Guid, FileState> stateMap = (await storeMetadata.GetFileStateList()).ToDictionary(x => x.MediaFileID);
        HashSet<Guid> heldSet = [.. await storeFile.GetHeldIDList()];

        // A held file is kept only when the library still has its media file and its state says it is on the device.
        foreach (Guid id in heldSet.ToList())
        {
            if (mediaMap.ContainsKey(id) && stateMap.TryGetValue(id, out FileState? state) && IsHeld(state)) continue;

            await storeFile.Delete(id);
            heldSet.Remove(id);
        }

        foreach (FileState state in stateMap.Values)
        {
            bool leftOver = state.State == FileSyncStateEnum.Downloading;
            bool claimsAFileThatIsGone = state.State == FileSyncStateEnum.OnDevice && !heldSet.Contains(state.MediaFileID);
            if (leftOver || claimsAFileThatIsGone) await storeMetadata.DeleteFileState(state.MediaFileID);
        }
    }

    private static bool IsHeld(FileState state)
    {
        return state.State == FileSyncStateEnum.OnDevice;
    }

    // The media file is gone: stop its download, drop what is on the disk for it, and let it be NotDownloaded.
    private async Task Forget(Guid id)
    {
        await serviceDownload.Cancel(id);
        await storeFile.Delete(id);
        await storeFile.DiscardPartial(id);
        await storeMetadata.DeleteFileState(id);
    }
}
