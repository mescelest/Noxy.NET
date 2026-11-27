using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class FileUseService : IFileUseService
{
    // Guarded by _lock. A write and the player's claim on a file are decided under the same lock, so one of them always sees the other: a write that began first is waited for, a claim that came first refuses the write.
    private readonly Lock _lock = new();
    private Guid? _inUseID;
    private Guid? _writingID;
    private TaskCompletionSource? _writeDone;

    public event Action? Changed;

    public async Task Use(Guid? mediaFileID)
    {
        Task? writeDone;
        bool released;
        lock (_lock)
        {
            released = _inUseID is not null && _inUseID != mediaFileID;
            _inUseID = mediaFileID;
            writeDone = mediaFileID is not null && _writingID == mediaFileID ? _writeDone?.Task : null;
        }

        if (writeDone is not null) await writeDone;

        if (released) Changed?.Invoke();
    }

    public bool TryBeginWrite(Guid mediaFileID)
    {
        lock (_lock)
        {
            if (_inUseID == mediaFileID) return false;

            _writingID = mediaFileID;
            _writeDone = new(TaskCreationOptions.RunContinuationsAsynchronously);

            return true;
        }
    }

    public void EndWrite(Guid mediaFileID)
    {
        TaskCompletionSource? done;
        lock (_lock)
        {
            if (_writingID != mediaFileID) return;

            done = _writeDone;
            _writingID = null;
            _writeDone = null;
        }

        done?.TrySetResult();
    }
}
