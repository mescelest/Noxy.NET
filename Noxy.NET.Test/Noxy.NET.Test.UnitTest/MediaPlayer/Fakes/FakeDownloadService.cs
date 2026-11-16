using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeDownloadService : IDownloadService
{
    public List<Guid> CancelList { get; } = [];

    public List<Guid> EnsureList { get; } = [];

    // What EnsureOnDevice answers.
    public bool EnsureResult { get; set; } = true;

    // When set, EnsureOnDevice runs it instead of answering EnsureResult, so a test can wait on a gate and put the file in the store.
    public Func<Guid, Task<bool>>? EnsureHandler { get; set; }

    // What Enqueue was asked for, in order, one list per call.
    public List<List<Guid>> EnqueueCallList { get; } = [];

    public event Action? Changed;

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public Task<bool> EnsureOnDevice(Guid mediaFileID)
    {
        EnsureList.Add(mediaFileID);

        return EnsureHandler is null ? Task.FromResult(EnsureResult) : EnsureHandler(mediaFileID);
    }

    public Task Enqueue(List<Guid> mediaFileIDList)
    {
        EnqueueCallList.Add([.. mediaFileIDList]);

        return Task.CompletedTask;
    }

    public Task Cancel(Guid mediaFileID)
    {
        CancelList.Add(mediaFileID);

        return Task.CompletedTask;
    }

    public List<Guid> QueueMissingList { get; } = [];

    public DownloadProgress Progress { get; set; } = new(0, 0, 0, 0, null);

    public Task QueueMissing(List<Guid> mediaFileIDList)
    {
        QueueMissingList.AddRange(mediaFileIDList);

        return Task.CompletedTask;
    }

    public DownloadProgress GetProgress()
    {
        return Progress;
    }
}
