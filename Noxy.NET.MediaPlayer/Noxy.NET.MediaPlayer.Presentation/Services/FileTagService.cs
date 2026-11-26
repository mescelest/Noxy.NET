using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class FileTagService(
    IFileStore storeFile,
    IMetadataStore storeMetadata,
    IMetadataListStore storeMetadataList,
    IMediaTagWritingService serviceMediaTagWriting,
    IMediaTagReadingService serviceMediaTagReading,
    ILogger<FileTagService> logger) : IFileTagService
{
    // One check or write at a time, so no file is read while it is being written.
    private readonly SemaphoreSlim _gate = new(1, 1);

    // The media files whose tags a check found different, with the TimeModified the check was made for. Not stored: a start checks them again.
    private readonly Lock _differenceLock = new();
    private readonly Dictionary<Guid, DateTime> _differenceMap = [];

    public event Action? Changed;

    public async Task CheckFiles()
    {
        bool changed;
        await _gate.WaitAsync();
        try
        {
            changed = await Check();
        }
        finally
        {
            _gate.Release();
        }

        if (changed) Changed?.Invoke();
    }

    public async Task<List<Guid>> GetOutdatedIDList()
    {
        List<KeyValuePair<Guid, DateTime>> differenceList;
        lock (_differenceLock) differenceList = [.. _differenceMap];

        // Nothing differs: the common case, which must not read the library, because the state bridge asks after every change of a download.
        if (differenceList.Count == 0) return [];

        Dictionary<Guid, FileState> stateMap = (await storeMetadata.GetFileStateList()).ToDictionary(x => x.MediaFileID);
        List<Guid> idList = [];
        foreach ((Guid id, DateTime checkedTimeModified) in differenceList)
        {
            ResponseMediaFile? media = await storeMetadata.GetMediaFile(id);
            if (media is not null && media.TimeModified == checkedTimeModified && stateMap.TryGetValue(id, out FileState? state) && state.State == FileSyncStateEnum.OnDevice && state.TagTimeModified != media.TimeModified) idList.Add(id);
        }

        return idList;
    }

    public async Task<BulkResult> Update(List<Guid> mediaFileIDList)
    {
        int succeededCount = 0;
        List<Guid> failedList = [];
        string? message = null;

        await _gate.WaitAsync();
        try
        {
            (Dictionary<Guid, string> artistNameMap, Dictionary<Guid, string> genreNameMap) = await LoadNameMapPair();
            List<FileState> stateList = await storeMetadata.GetFileStateList();

            foreach (Guid id in mediaFileIDList.Distinct())
            {
                ResponseMediaFile? media = await storeMetadata.GetMediaFile(id);
                FileState? state = stateList.FirstOrDefault(x => x.MediaFileID == id);
                if (media is null || state is not { State: FileSyncStateEnum.OnDevice } || await storeFile.GetPath(id) is null)
                {
                    failedList.Add(id);
                    message ??= PresentationTexts.ActionMessageNotOnDevice;
                    continue;
                }

                MediaTagValues? values = media.ToTagValues(artistNameMap, genreNameMap);
                if (values is null)
                {
                    failedList.Add(id);
                    message ??= PresentationTexts.ActionMessageMetadataNotLoaded;
                    continue;
                }

                try
                {
                    await Task.Run(() => storeFile.Rewrite(id, path => serviceMediaTagWriting.Write(path, values)));
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    // The exception type only: its message can quote what was in the file.
                    logger.LogWarning("The metadata could not be written to a file ({ExceptionType}).", exception.GetType().Name);
                    failedList.Add(id);
                    message ??= exception is StorageFullException ? PresentationTexts.DownloadDiskFull : PresentationTexts.ActionMessageUpdateFileFailed;
                    continue;
                }

                // The time of the media file as it was read before the write: an edit that arrived meanwhile leaves the file out of date.
                await storeMetadata.SaveFileState(state with { TagTimeModified = media.TimeModified });
                lock (_differenceLock) _differenceMap.Remove(id);
                succeededCount++;
            }
        }
        finally
        {
            _gate.Release();
        }

        if (succeededCount > 0) Changed?.Invoke();

        return new(succeededCount, failedList, message);
    }

    public async Task<BulkResult> UpdateOutdated()
    {
        return await Update(await GetOutdatedIDList());
    }

    private async Task<bool> Check()
    {
        List<FileState> stateList = await storeMetadata.GetFileStateList();
        Dictionary<Guid, ResponseMediaFile> mediaMap = (await storeMetadata.GetMediaFileList()).ToDictionary(x => x.ID);
        HashSet<Guid> onDeviceSet = [.. stateList.Where(x => x.State == FileSyncStateEnum.OnDevice).Select(x => x.MediaFileID)];

        // A file that left the device or was downloaded again is read again when it is back.
        bool changed = ForgetDifferences(onDeviceSet);

        (Dictionary<Guid, string> artistNameMap, Dictionary<Guid, string> genreNameMap) = await LoadNameMapPair();
        foreach (FileState state in stateList.Where(x => x.State == FileSyncStateEnum.OnDevice))
        {
            if (!mediaMap.TryGetValue(state.MediaFileID, out ResponseMediaFile? media) || state.TagTimeModified == media.TimeModified) continue;
            if (IsKnownDifferent(state.MediaFileID, media.TimeModified)) continue;

            string? path = await storeFile.GetPath(state.MediaFileID);
            MediaTagValues? values = media.ToTagValues(artistNameMap, genreNameMap);
            if (path is null || values is null) continue;

            MediaTagSet? tags = await ReadTags(path);
            if (tags is null) continue;

            if (tags.Matches(values))
            {
                await storeMetadata.SaveFileState(state with { TagTimeModified = media.TimeModified });
                lock (_differenceLock) _differenceMap.Remove(state.MediaFileID);
            }
            else
            {
                lock (_differenceLock) _differenceMap[state.MediaFileID] = media.TimeModified;
            }

            changed = true;
        }

        return changed;
    }

    private bool ForgetDifferences(HashSet<Guid> onDeviceSet)
    {
        lock (_differenceLock)
        {
            List<Guid> goneList = [.. _differenceMap.Keys.Where(x => !onDeviceSet.Contains(x))];
            foreach (Guid id in goneList) _differenceMap.Remove(id);

            return goneList.Count > 0;
        }
    }

    private bool IsKnownDifferent(Guid mediaFileID, DateTime timeModified)
    {
        lock (_differenceLock) return _differenceMap.TryGetValue(mediaFileID, out DateTime known) && known == timeModified;
    }

    // Null when the file cannot be opened right now (locked): it stays unchecked and is tried again next time.
    private async Task<MediaTagSet?> ReadTags(string path)
    {
        try
        {
            return await Task.Run(() => serviceMediaTagReading.Read(path));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning("The tags of a file could not be read ({ExceptionType}).", exception.GetType().Name);
            return null;
        }
    }

    private async Task<(Dictionary<Guid, string> ArtistNameMap, Dictionary<Guid, string> GenreNameMap)> LoadNameMapPair()
    {
        Dictionary<Guid, string> artistNameMap = (await storeMetadataList.Artist.GetList()).ToDictionary(x => x.ID, x => x.Name);
        Dictionary<Guid, string> genreNameMap = (await storeMetadataList.Genre.GetList()).ToDictionary(x => x.ID, x => x.Name);

        return (artistNameMap, genreNameMap);
    }
}
