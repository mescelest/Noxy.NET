using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public sealed class UploadService : IUploadService, IDisposable
{
    private sealed class Batch(Guid? playlistID, int total)
    {
        public Guid? PlaylistID { get; } = playlistID;
        public int Remaining { get; set; } = total;
        public int SucceededCount { get; set; }
        public int DuplicateCount { get; set; }
        public int ExistingAddedCount { get; set; }
        public List<UploadFailure> FailureList { get; } = [];
    }

    private readonly IApiClient _clientApi;
    private readonly ISyncService _serviceSync;
    private readonly IMetadataStore _storeMetadata;
    private readonly Lock _lock = new();
    private readonly Dictionary<Guid, Batch> _jobMap = [];

    // A result can arrive before the answer to the request that started the job: it waits here until the job is known.
    private readonly Dictionary<Guid, ResponseUploadResult> _earlyMap = [];
    private int _activeCount;

    public event Action<int>? Started;

    public event Action<UploadBatchResult>? BatchCompleted;

    public UploadService(IApiClient clientApi, ISyncService serviceSync, IMetadataStore storeMetadata)
    {
        _clientApi = clientApi;
        _serviceSync = serviceSync;
        _storeMetadata = storeMetadata;
        _serviceSync.UploadResultReceived += OnUploadResultReceived;
    }

    public async Task Upload(List<UploadFile> fileList, Guid? playlistID = null)
    {
        if (fileList.Count == 0) return;

        Batch batch = new(playlistID, fileList.Count);
        Started?.Invoke(fileList.Count);
        lock (_lock) _activeCount++;

        try
        {
            foreach (UploadFile file in fileList)
            {
                await Send(batch, file);
            }
        }
        finally
        {
            lock (_lock)
            {
                _activeCount--;

                // Every job of this call is known by now, so a result still waiting here belongs to another device's upload.
                if (_activeCount == 0) _earlyMap.Clear();
            }
        }
    }

    public void Dispose()
    {
        _serviceSync.UploadResultReceived -= OnUploadResultReceived;
    }

    private async Task Send(Batch batch, UploadFile file)
    {
        ResponseUploadAccepted accepted;
        try
        {
            accepted = await _clientApi.UploadMedia(file.Name, file.ContentType, file.OpenContent);
        }
        catch (Exception exception) when (exception is ApiException || exception.IsOfflineFailure)
        {
            Finish(batch, UploadOutcomeEnum.Failed, new(file.Name, exception.ToActionMessage(PresentationTexts.ActionMessageUploadFailed)));
            return;
        }

        ResponseUploadResult? early;
        lock (_lock)
        {
            if (!_earlyMap.Remove(accepted.JobID, out early))
            {
                _jobMap[accepted.JobID] = batch;
            }
        }

        if (early is not null) await Handle(batch, early);
    }

    private void OnUploadResultReceived(ResponseUploadResult result)
    {
        Batch? batch;
        lock (_lock)
        {
            if (!_jobMap.Remove(result.JobID, out batch))
            {
                if (_activeCount > 0) _earlyMap[result.JobID] = result;

                return;
            }
        }

        _ = Handle(batch, result);
    }

    private async Task Handle(Batch batch, ResponseUploadResult result)
    {
        // A file the library already held is not a failure and is not stored again. Sent to a playlist it still goes into the playlist, by the rules of that playlist.
        bool isDuplicate = result.Reason == UploadFailureReasonEnum.Duplicate;

        if (!isDuplicate && (!result.Succeeded || result.Media is null))
        {
            Finish(batch, UploadOutcomeEnum.Failed, new(result.Title, ReasonMessageOf(result.Reason)));
            return;
        }

        if (batch.PlaylistID is not { } playlistID || result.Media is null)
        {
            Finish(batch, isDuplicate ? UploadOutcomeEnum.AlreadyInLibrary : UploadOutcomeEnum.Added, null);
            return;
        }

        if (isDuplicate && await PlaylistRefuses(playlistID, result.Media.ID))
        {
            Finish(batch, UploadOutcomeEnum.Failed, new(result.Title, PresentationTexts.UploadMessageAlreadyInPlaylist));
            return;
        }

        try
        {
            await _clientApi.AddPlaylistEntryList(playlistID, new() { MediaFileIDList = [result.Media.ID] });
        }
        catch (Exception exception) when (exception is ApiException || exception.IsOfflineFailure)
        {
            Finish(batch, UploadOutcomeEnum.Failed, new(result.Title, PresentationTexts.UploadMessageNotAddedToPlaylist));
            return;
        }

        Finish(batch, isDuplicate ? UploadOutcomeEnum.AlreadyInLibraryAddedToPlaylist : UploadOutcomeEnum.Added, null);
    }

    // A playlist that does not take duplicates keeps its one entry of a media file, and the server then adds nothing; the app has to tell the user. The answer comes from the playlist as the device has it.
    private async Task<bool> PlaylistRefuses(Guid playlistID, Guid mediaFileID)
    {
        ResponsePlaylistDetail? detail = await _storeMetadata.GetPlaylistDetail(playlistID);

        return detail is { AllowDuplicates: false } && detail.EntryList.Any(x => x.MediaFileID == mediaFileID);
    }

    // One file of the batch has its final answer. The batch is reported when the last one is in.
    private void Finish(Batch batch, UploadOutcomeEnum outcome, UploadFailure? failure)
    {
        UploadBatchResult? completed = null;
        lock (_lock)
        {
            switch (outcome)
            {
                case UploadOutcomeEnum.Added:
                    batch.SucceededCount++;
                    break;
                case UploadOutcomeEnum.AlreadyInLibrary:
                    batch.DuplicateCount++;
                    break;
                case UploadOutcomeEnum.AlreadyInLibraryAddedToPlaylist:
                    batch.DuplicateCount++;
                    batch.ExistingAddedCount++;
                    break;
                case UploadOutcomeEnum.Failed:
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(outcome), (int)outcome, typeof(UploadOutcomeEnum));
            }

            if (failure is not null) batch.FailureList.Add(failure);

            batch.Remaining--;
            if (batch.Remaining == 0) completed = new(batch.PlaylistID, batch.SucceededCount, batch.DuplicateCount, batch.ExistingAddedCount, [.. batch.FailureList]);
        }

        if (completed is not null) BatchCompleted?.Invoke(completed);
    }

    private static string ReasonMessageOf(UploadFailureReasonEnum? reason)
    {
        return reason switch
        {
            null or UploadFailureReasonEnum.Unknown => PresentationTexts.UploadMessageUnknown,
            UploadFailureReasonEnum.LibraryFull => PresentationTexts.UploadMessageLibraryFull,
            UploadFailureReasonEnum.FileMissing => PresentationTexts.UploadMessageFileMissing,
            UploadFailureReasonEnum.Duplicate => PresentationTexts.UploadMessageDuplicate,
            _ => throw new InvalidEnumArgumentException(nameof(reason), (int)reason.Value, typeof(UploadFailureReasonEnum))
        };
    }
}
