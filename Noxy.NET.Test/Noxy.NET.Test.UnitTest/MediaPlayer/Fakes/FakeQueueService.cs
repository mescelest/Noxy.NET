using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeQueueService : IQueueService
{
    public event Action? Changed;

    public event Action<QueueEdit>? EditApplied;

    public event Action? GapDetected;

    public event Action? CurrentRemoved;

    public LiveQueue Queue { get; set; } = new();

    public bool OwnsPlayback { get; set; }

    // What Apply answers.
    public bool ApplyResult { get; set; } = true;

    // What StartSource answers.
    public bool StartSourceResult { get; set; } = true;

    // What LoadLocalCopy answers.
    public TimeSpan? LocalCopyPosition { get; set; }

    public List<QueueEdit> ApplyList { get; } = [];

    public List<ResponseQueueDelta> DeltaList { get; } = [];

    public List<(ResponseQueue Snapshot, bool KeepLocalPointer)> SnapshotList { get; } = [];

    public List<(ResponseQueueSource Source, Guid? StartID, ShuffleModeEnum Shuffle)> StartSourceList { get; } = [];

    public List<ShuffleModeEnum> RegenerateList { get; } = [];

    public List<double> SavedPositionList { get; } = [];

    public int LoadLocalCopyCount { get; private set; }

    // Thrown by LoadLocalCopy when set.
    public Exception? LocalCopyFailure { get; set; }

    public int ClearCount { get; private set; }

    public void RaiseGapDetected()
    {
        GapDetected?.Invoke();
    }

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public void RaiseCurrentRemoved()
    {
        CurrentRemoved?.Invoke();
    }

    public void RaiseEditApplied(QueueEdit edit)
    {
        EditApplied?.Invoke(edit);
    }

    public Task<bool> Apply(QueueEdit edit)
    {
        ApplyList.Add(edit);
        if (ApplyResult) EditApplied?.Invoke(edit);

        return Task.FromResult(ApplyResult);
    }

    public Task ApplyDelta(ResponseQueueDelta delta)
    {
        DeltaList.Add(delta);
        return Task.CompletedTask;
    }

    public Task ReplaceWithSnapshot(ResponseQueue snapshot, bool keepLocalPointer)
    {
        SnapshotList.Add((snapshot, keepLocalPointer));
        return Task.CompletedTask;
    }

    public Task<bool> StartSource(ResponseQueueSource source, Guid? startID, ShuffleModeEnum shuffle)
    {
        StartSourceList.Add((source, startID, shuffle));
        return Task.FromResult(StartSourceResult);
    }

    public Task Regenerate(ShuffleModeEnum shuffle)
    {
        RegenerateList.Add(shuffle);
        return Task.CompletedTask;
    }

    public Task<TimeSpan?> LoadLocalCopy()
    {
        LoadLocalCopyCount++;
        if (LocalCopyFailure is not null) throw LocalCopyFailure;

        return Task.FromResult(LocalCopyPosition);
    }

    public Task SaveLocalCopy(double positionSeconds)
    {
        SavedPositionList.Add(positionSeconds);
        return Task.CompletedTask;
    }

    public Task Clear()
    {
        ClearCount++;
        return Task.CompletedTask;
    }
}
