using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class FileReconciler(IFileStore storeFile, IMetadataStore storeMetadata, IDownloadService serviceDownload) : IFileReconciler
{
    public async Task Apply(LibraryDiff diff)
    {
        foreach (Guid id in diff.RemovedIDList) await Forget(id);
        foreach (ResponseMediaFile changed in diff.ChangedList) await Forget(changed.ID);
    }

    public async Task Repair()
    {
        foreach (Guid id in await storeFile.GetPartialIDList()) await storeFile.DiscardPartial(id);

        Dictionary<Guid, ResponseMediaFile> mediaMap = (await storeMetadata.GetMediaFileList()).ToDictionary(x => x.ID);
        Dictionary<Guid, FileState> stateMap = (await storeMetadata.GetFileStateList()).ToDictionary(x => x.MediaFileID);
        HashSet<Guid> heldSet = [.. await storeFile.GetHeldIDList()];

        // A held file is kept only when the library still has its media file and it was verified against the library's current hash.
        foreach (Guid id in heldSet.ToList())
        {
            if (mediaMap.TryGetValue(id, out ResponseMediaFile? media) && stateMap.TryGetValue(id, out FileState? state) && IsVerified(state, media)) continue;

            await storeFile.Delete(id);
            heldSet.Remove(id);
        }

        foreach (FileState state in stateMap.Values)
        {
            bool leftOver = state.State == FileSyncState.Downloading;
            bool claimsAFileThatIsGone = state.State == FileSyncState.OnDevice && !heldSet.Contains(state.MediaFileID);
            if (leftOver || claimsAFileThatIsGone) await storeMetadata.DeleteFileState(state.MediaFileID);
        }
    }

    private static bool IsVerified(FileState state, ResponseMediaFile media)
    {
        return state.State == FileSyncState.OnDevice && string.Equals(state.VerifiedHash, media.ContentHash, StringComparison.OrdinalIgnoreCase);
    }

    // The media file is gone or has other bytes now: stop its download, drop what is on the disk for it, and let it be NotDownloaded.
    private async Task Forget(Guid id)
    {
        await serviceDownload.Cancel(id);
        await storeFile.Delete(id);
        await storeFile.DiscardPartial(id);
        await storeMetadata.DeleteFileState(id);
    }
}
