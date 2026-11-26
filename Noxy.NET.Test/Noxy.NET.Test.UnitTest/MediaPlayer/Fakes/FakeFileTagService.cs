using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeFileTagService : IFileTagService
{
    public event Action? Changed;

    public int CheckCount { get; private set; }
    public int MaxActiveCount { get; private set; }

    // When set, CheckFiles waits for it, so a test can hold a run open.
    public TaskCompletionSource? CheckGate { get; set; }

    private int _activeCount;
    public int UpdateOutdatedCount { get; private set; }
    public List<List<Guid>> UpdateList { get; } = [];

    public List<Guid> OutdatedIDList { get; set; } = [];
    public BulkResult Result { get; set; } = new(0, [], null);

    // Thrown by CheckFiles and UpdateOutdated.
    public Exception? Failure { get; set; }

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public async Task CheckFiles()
    {
        CheckCount++;
        MaxActiveCount = Math.Max(MaxActiveCount, Interlocked.Increment(ref _activeCount));
        try
        {
            if (CheckGate is not null) await CheckGate.Task;
            if (Failure is not null) throw Failure;
        }
        finally
        {
            Interlocked.Decrement(ref _activeCount);
        }
    }

    public Task<BulkResult> Update(List<Guid> mediaFileIDList)
    {
        UpdateList.Add([.. mediaFileIDList]);

        return Task.FromResult(Result);
    }

    public Task<List<Guid>> GetOutdatedIDList()
    {
        return Task.FromResult(OutdatedIDList);
    }

    public Task<BulkResult> UpdateOutdated()
    {
        UpdateOutdatedCount++;
        if (Failure is not null) throw Failure;

        return Task.FromResult(Result);
    }
}
