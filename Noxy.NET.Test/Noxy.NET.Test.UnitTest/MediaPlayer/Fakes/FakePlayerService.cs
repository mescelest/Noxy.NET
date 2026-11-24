using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakePlayerService : IPlayerService
{
    public event Action? Changed;

    public QueueItem? Current { get; set; }

    public LiveQueue Queue { get; set; } = new();

    public bool IsPlaying { get; set; }

    // Whether a media file is loaded: Resume does nothing without one, as the real player.
    public bool IsLoaded { get; set; } = true;

    public bool IsDisabled => Current is null;

    public TimeSpan Position { get; set; }

    public TimeSpan Duration { get; set; }

    public double Volume { get; set; } = 1;

    public bool IsWaiting { get; set; }

    public ShuffleModeEnum Shuffle { get; set; } = ShuffleModeEnum.Off;

    public RepeatModeEnum Repeat { get; set; } = RepeatModeEnum.No;

    public Guid? PlaylistID { get; set; }

    public bool PositionJumped { get; set; }

    // Every call in order, as the method name with its values.
    public List<string> CallList { get; } = [];

    public FileSyncStateEnum StateOf(QueueItem item)
    {
        return FileSyncStateEnum.OnDevice;
    }

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public Task StartLibrary(Guid mediaFileID, LibrarySortEnum sort, bool descending, string search)
    {
        CallList.Add($"StartLibrary {mediaFileID} {sort} {descending} {search}");
        return Task.CompletedTask;
    }

    public Task StartPlaylist(Guid playlistID, Guid? startEntryID = null)
    {
        CallList.Add($"StartPlaylist {playlistID} {startEntryID}");
        return Task.CompletedTask;
    }

    // When set, PlayCurrent waits for it, as a download that is still running would hold it.
    public Task? PlayCurrentGate { get; set; }

    public async Task PlayCurrent()
    {
        CallList.Add("PlayCurrent");
        if (PlayCurrentGate is not null) await PlayCurrentGate;

        IsLoaded = Current is not null;

        IsPlaying = Current is not null;
        Changed?.Invoke();
    }

    public Task Pause()
    {
        CallList.Add("Pause");
        IsPlaying = false;
        Changed?.Invoke();

        return Task.CompletedTask;
    }

    // When set, a resume does not show as playing at once, as the real player that is still starting.
    public bool ResumeIsSlow { get; set; }

    public Task<bool> Resume()
    {
        CallList.Add("Resume");
        if (IsLoaded && !ResumeIsSlow) IsPlaying = Current is not null;

        Changed?.Invoke();

        return Task.FromResult(IsLoaded);
    }

    public Task Seek(TimeSpan position)
    {
        CallList.Add($"Seek {position}");
        Position = position;
        PositionJumped = true;
        Changed?.Invoke();

        return Task.CompletedTask;
    }

    public Task JumpTo(Guid itemID, bool keepSkipped)
    {
        CallList.Add($"JumpTo {itemID} {keepSkipped}");
        return Task.CompletedTask;
    }

    // Thrown by Next when set.
    public Exception? NextFailure { get; set; }

    public Task Next()
    {
        CallList.Add("Next");
        if (NextFailure is not null) throw NextFailure;

        return Task.CompletedTask;
    }

    public Task Previous()
    {
        CallList.Add("Previous");
        return Task.CompletedTask;
    }

    public Task LoadVolume()
    {
        CallList.Add("LoadVolume");
        return Task.CompletedTask;
    }

    public Task SetVolume(double volume)
    {
        CallList.Add($"SetVolume {volume}");
        Volume = volume;
        Changed?.Invoke();

        return Task.CompletedTask;
    }

    public Task PreviewVolume(double volume)
    {
        CallList.Add($"PreviewVolume {volume}");
        Volume = volume;
        Changed?.Invoke();

        return Task.CompletedTask;
    }

    public Task SetShuffle(ShuffleModeEnum shuffle)
    {
        CallList.Add($"SetShuffle {shuffle}");
        Shuffle = shuffle;
        Changed?.Invoke();

        return Task.CompletedTask;
    }

    public Task SetRepeat(RepeatModeEnum repeat)
    {
        CallList.Add($"SetRepeat {repeat}");
        Repeat = repeat;
        Changed?.Invoke();

        return Task.CompletedTask;
    }

    public Task Clear()
    {
        CallList.Add("Clear");
        Current = null;
        IsPlaying = false;
        Changed?.Invoke();

        return Task.CompletedTask;
    }
}
