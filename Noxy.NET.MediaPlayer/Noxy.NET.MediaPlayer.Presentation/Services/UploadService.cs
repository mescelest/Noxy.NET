using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public sealed class UploadService : IUploadService, IDisposable
{
    private sealed class Batch(Guid? playlistID, int total)
    {
        public Guid? PlaylistID { get; } = playlistID;
        public int Remaining { get; set; } = total;
        public int SucceededCount { get; set; }
        public List<UploadFailure> FailureList { get; } = [];
    }

    private readonly IApiClient _clientApi;
    private readonly ISyncService _serviceSync;
    private readonly Lock _lock = new();
    private readonly Dictionary<Guid, Batch> _jobMap = [];

    // A result can arrive before the answer to the request that started the job: it waits here until the job is known.
    private readonly Dictionary<Guid, ResponseUploadResult> _earlyMap = [];
    private int _activeCount;

    public event Action<int>? Started;

    public event Action<UploadBatchResult>? BatchCompleted;

    public UploadService(IApiClient clientApi, ISyncService serviceSync)
    {
        _clientApi = clientApi;
        _serviceSync = serviceSync;
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
            Finish(batch, null, new(file.Name, exception.ToActionMessage(PresentationTexts.ActionMessageUploadFailed)));
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
        if (!result.Succeeded || result.Media is null)
        {
            Finish(batch, null, new(result.Title, ReasonMessageOf(result.Reason)));
            return;
        }

        if (batch.PlaylistID is { } playlistID)
        {
            try
            {
                await _clientApi.AddPlaylistEntryList(playlistID, new() { MediaFileIDList = [result.Media.ID] });
            }
            catch (Exception exception) when (exception is ApiException || exception.IsOfflineFailure)
            {
                Finish(batch, null, new(result.Title, PresentationTexts.UploadMessageNotAddedToPlaylist));
                return;
            }
        }

        Finish(batch, true, null);
    }

    // One file of the batch has its final answer: it is in (succeeded), or it is not (failure). The batch is reported when the last one is in.
    private void Finish(Batch batch, bool? succeeded, UploadFailure? failure)
    {
        UploadBatchResult? completed = null;
        lock (_lock)
        {
            if (succeeded is true) batch.SucceededCount++;
            if (failure is not null) batch.FailureList.Add(failure);

            batch.Remaining--;
            if (batch.Remaining == 0) completed = new(batch.PlaylistID, batch.SucceededCount, [.. batch.FailureList]);
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
            _ => throw new InvalidEnumArgumentException(nameof(reason), (int)reason.Value, typeof(UploadFailureReasonEnum))
        };
    }
}
