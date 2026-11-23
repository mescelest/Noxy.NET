using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakePlaybackControlService : IPlaybackControlService
{
    public event Action? TransferFailed;

    public bool CanContinue { get; set; }

    // What Continue answers.
    public bool ContinueResult { get; set; } = true;

    // Thrown by Transfer when set.
    public Exception? TransferFailure { get; set; }

    // Every call in order, as the method name with its values.
    public List<string> CallList { get; } = [];

    public void RaiseTransferFailed()
    {
        TransferFailed?.Invoke();
    }

    public Task Play()
    {
        CallList.Add("Play");
        return Task.CompletedTask;
    }

    public Task Pause()
    {
        CallList.Add("Pause");
        return Task.CompletedTask;
    }

    public Task Seek(TimeSpan position)
    {
        CallList.Add($"Seek {position}");
        return Task.CompletedTask;
    }

    public Task Next()
    {
        CallList.Add("Next");
        return Task.CompletedTask;
    }

    public Task Previous()
    {
        CallList.Add("Previous");
        return Task.CompletedTask;
    }

    public Task SetVolume(int volume)
    {
        CallList.Add($"SetVolume {volume}");
        return Task.CompletedTask;
    }

    public Task PreviewVolume(int volume)
    {
        CallList.Add($"PreviewVolume {volume}");
        return Task.CompletedTask;
    }

    public Task SetShuffle(ShuffleModeEnum shuffle)
    {
        CallList.Add($"SetShuffle {shuffle}");
        return Task.CompletedTask;
    }

    public Task SetRepeat(RepeatModeEnum repeat)
    {
        CallList.Add($"SetRepeat {repeat}");
        return Task.CompletedTask;
    }

    public Task PlayNext(Guid mediaFileID)
    {
        CallList.Add($"PlayNext {mediaFileID}");
        return Task.CompletedTask;
    }

    public Task AddToQueue(Guid mediaFileID)
    {
        CallList.Add($"AddToQueue {mediaFileID}");
        return Task.CompletedTask;
    }

    // Runs inside RemoveFromQueue, so a test can record the order of calls.
    public Action<Guid>? RemoveFromQueueHandler { get; set; }

    public Task RemoveFromQueue(Guid itemID)
    {
        CallList.Add($"RemoveFromQueue {itemID}");
        RemoveFromQueueHandler?.Invoke(itemID);
        return Task.CompletedTask;
    }

    public Task MoveInQueue(Guid itemID, Guid? afterItemID)
    {
        CallList.Add($"MoveInQueue {itemID} {afterItemID}");
        return Task.CompletedTask;
    }

    public Task JumpTo(Guid itemID, bool keepSkipped)
    {
        CallList.Add($"JumpTo {itemID} {keepSkipped}");
        return Task.CompletedTask;
    }

    public Task StartLibrary(Guid mediaFileID, LibrarySortEnum sort, bool descending, string search)
    {
        CallList.Add($"StartLibrary {mediaFileID} {sort} {descending} {search}");
        return Task.CompletedTask;
    }

    public Task StartPlaylist(Guid playlistID, Guid? entryID = null)
    {
        CallList.Add($"StartPlaylist {playlistID} {entryID}");
        return Task.CompletedTask;
    }

    public Task Transfer(Guid deviceID)
    {
        CallList.Add($"Transfer {deviceID}");
        if (TransferFailure is not null) throw TransferFailure;

        return Task.CompletedTask;
    }

    public Task<bool> Continue()
    {
        CallList.Add("Continue");
        return Task.FromResult(ContinueResult);
    }
}
