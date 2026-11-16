using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeFileReconciler : IFileReconciler
{
    public List<LibraryDiff> ApplyList { get; } = [];

    public int RepairCount { get; private set; }

    // Runs inside Repair, so a test can record what had happened by then.
    public Action? OnRepair { get; set; }

    public Task Apply(LibraryDiff diff)
    {
        ApplyList.Add(diff);

        return Task.CompletedTask;
    }

    public Task Repair()
    {
        RepairCount++;
        OnRepair?.Invoke();

        return Task.CompletedTask;
    }
}
