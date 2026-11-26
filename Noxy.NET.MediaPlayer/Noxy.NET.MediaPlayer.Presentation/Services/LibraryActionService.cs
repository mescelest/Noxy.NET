using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class LibraryActionService(
    IDownloadService serviceDownload,
    IQueueService serviceQueue,
    IPlaybackControlService serviceControl,
    IApiClient clientApi,
    ISyncService serviceSync,
    IMetadataStore storeMetadata,
    IFileSaver serviceFileSaver,
    IFileStore storeFile,
    IFolderOpener serviceFolderOpener,
    IFileTagService serviceFileTag)
    : ILibraryActionService
{
    public async Task<BulkResult> Download(List<Guid> mediaFileIDList)
    {
        if (mediaFileIDList.Count == 0) return new(0, [], null);

        await serviceDownload.QueueMissing(mediaFileIDList);

        return new(mediaFileIDList.Count, [], null);
    }

    // Each one goes in front of the ones already put next, so the list is walked backwards to keep its order.
    public async Task<BulkResult> PlayNext(List<Guid> mediaFileIDList)
    {
        foreach (Guid id in Enumerable.Reverse(mediaFileIDList)) await serviceControl.PlayNext(id);

        return new(mediaFileIDList.Count, [], null);
    }

    public async Task<BulkResult> AddToQueue(List<Guid> mediaFileIDList)
    {
        foreach (Guid id in mediaFileIDList) await serviceControl.AddToQueue(id);

        return new(mediaFileIDList.Count, [], null);
    }

    public bool CanOpenFolder => serviceFolderOpener.IsSupported;

    public async Task<BulkResult> OpenContainingFolder(List<Guid> mediaFileIDList)
    {
        if (mediaFileIDList.Count == 0) return new(0, [], null);

        foreach (Guid id in mediaFileIDList)
        {
            string? path = await storeFile.GetPath(id);
            if (path is null) continue;

            try
            {
                await serviceFolderOpener.Reveal(path);
            }
            catch (IOException)
            {
                return new(0, [.. mediaFileIDList], PresentationTexts.ActionMessageOpenFolderFailed);
            }

            return new(1, [], null);
        }

        return new(0, [.. mediaFileIDList], PresentationTexts.ActionMessageNotOnDevice);
    }

    public async Task<BulkResult> UpdateFileMetadata(List<Guid> mediaFileIDList)
    {
        if (mediaFileIDList.Count == 0) return new(0, [], null);

        return await serviceFileTag.Update(mediaFileIDList);
    }

    public async Task<BulkResult> UpdateAllFiles()
    {
        return await serviceFileTag.UpdateOutdated();
    }

    public async Task<BulkResult> RemoveFromDevice(List<Guid> mediaFileIDList)
    {
        if (mediaFileIDList.Count == 0) return new(0, [], null);

        HashSet<Guid> idSet = [.. mediaFileIDList];

        // First out of the queue: the playing one then stops and lets go of its file, so the delete below cannot be refused.
        ResponseQueue queue = serviceQueue.GetSnapshot();
        List<Guid> itemIDList = [];
        if (queue.Current is { } current && idSet.Contains(current.MediaFileID)) itemIDList.Add(current.ItemID);

        itemIDList.AddRange(queue.HistoryList.Concat(queue.UserQueueList).Concat(queue.SourceList).Where(x => idSet.Contains(x.MediaFileID)).Select(x => x.ItemID));
        foreach (Guid itemID in itemIDList) await serviceControl.RemoveFromQueue(itemID);

        List<Guid> failedList = [];
        foreach (Guid id in idSet)
        {
            try
            {
                await serviceDownload.Remove(id);
            }
            catch (IOException)
            {
                failedList.Add(id);
            }
        }

        return new(idSet.Count - failedList.Count, failedList, failedList.Count == 0 ? null : PresentationTexts.ActionMessageRemoveFailed);
    }

    public async Task<BulkResult> DeleteFromLibrary(List<Guid> mediaFileIDList)
    {
        if (mediaFileIDList.Count == 0) return new(0, [], null);
        if (GetDisabledReason(LibraryActionEnum.DeleteFromLibrary) is { } reason) return new(0, [.. mediaFileIDList], reason);

        List<Guid> failedList = [];
        string? message = null;
        foreach (Guid id in mediaFileIDList)
        {
            try
            {
                await clientApi.DeleteMedia(id);
            }
            catch (Exception exception) when (IsExpected(exception))
            {
                failedList.Add(id);
                message ??= exception.ToActionMessage(PresentationTexts.ActionMessageDeleteFailed);
            }
        }

        return new(mediaFileIDList.Count - failedList.Count, failedList, message);
    }

    // One save dialog per media file. The first cancel ends the rest: the user said no.
    public async Task<BulkResult> SaveAsFile(List<Guid> mediaFileIDList)
    {
        if (mediaFileIDList.Count == 0) return new(0, [], null);
        if (GetDisabledReason(LibraryActionEnum.SaveAsFile) is { } reason) return new(0, [.. mediaFileIDList], reason);

        int savedCount = 0;
        List<Guid> failedList = [];
        string? message = null;
        foreach (Guid id in mediaFileIDList)
        {
            try
            {
                ResponseMediaFile? media = await storeMetadata.GetMediaFile(id);
                using ApiFileResponse response = await clientApi.GetTaggedFile(id);
                if (!await serviceFileSaver.Save(SuggestedName(media), response.Content)) break;

                savedCount++;
            }
            catch (Exception exception) when (IsExpected(exception) || exception is IOException)
            {
                failedList.Add(id);
                message ??= exception is IOException ? PresentationTexts.ActionMessageSaveFailed : exception.ToActionMessage(PresentationTexts.ActionMessageSaveFailed);
            }
        }

        return new(savedCount, failedList, message);
    }

    private static readonly HashSet<char> InvalidFileNameCharSet = [.. Path.GetInvalidFileNameChars()];

    // The title with the extension the downloader gives the same content type, and nothing a file name cannot hold.
    private static string SuggestedName(ResponseMediaFile? media)
    {
        string title = string.Concat((media?.Title ?? "Media").Select(x => InvalidFileNameCharSet.Contains(x) ? '_' : x)).Trim();

        return (string.IsNullOrEmpty(title) ? "Media" : title) + (media?.ContentType.GetFileExtension() ?? string.Empty);
    }

    // What a call to the server can throw without it being a bug: its own answer, or no answer at all.
    private static bool IsExpected(Exception exception)
    {
        return exception is ApiException || exception.IsOfflineFailure;
    }

    public async Task<BulkResult> AddToPlaylist(Guid playlistID, List<Guid> mediaFileIDList)
    {
        if (mediaFileIDList.Count == 0) return new(0, [], null);
        if (GetDisabledReason(LibraryActionEnum.AddToPlaylist) is { } reason) return new(0, [.. mediaFileIDList], reason);

        try
        {
            await clientApi.AddPlaylistEntryList(playlistID, new() { MediaFileIDList = [.. mediaFileIDList] });
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return new(0, [.. mediaFileIDList], exception.ToActionMessage(PresentationTexts.ActionMessageAddFailed));
        }

        return new(mediaFileIDList.Count, [], null);
    }

    public string? GetDisabledReason(LibraryActionEnum action)
    {
        switch (action)
        {
            case LibraryActionEnum.DeleteFromLibrary:
            case LibraryActionEnum.AddToPlaylist:
            case LibraryActionEnum.SaveAsFile:
            case LibraryActionEnum.Edit:
                return serviceSync.IsOffline ? PresentationTexts.ActionMessageOffline : null;
            case LibraryActionEnum.Download:
            case LibraryActionEnum.PlayNext:
            case LibraryActionEnum.AddToQueue:
            case LibraryActionEnum.RemoveFromDevice:
            case LibraryActionEnum.OpenFolder:
            case LibraryActionEnum.UpdateFileMetadata:
                return null;
            default:
                throw new InvalidEnumArgumentException(nameof(action), (int)action, typeof(LibraryActionEnum));
        }
    }
}
