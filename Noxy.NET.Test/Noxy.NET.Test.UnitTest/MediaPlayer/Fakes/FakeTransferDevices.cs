using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

// The sends a hub would make during a transfer, recorded, with the answers a real device would give.
public sealed class FakeTransferDevices
{
    public List<Guid> FinalStateRequestList { get; } = [];
    public List<(Guid Target, ResponsePlayerState State, ResponseQueue Queue)> StartList { get; } = [];
    public Func<Guid, Task>? OnFinalStateRequested { get; set; }
    public Func<Guid, ResponsePlayerState, ResponseQueue, Task>? OnStart { get; set; }

    public Task RequestFinalState(Guid deviceID)
    {
        FinalStateRequestList.Add(deviceID);
        return OnFinalStateRequested?.Invoke(deviceID) ?? Task.CompletedTask;
    }

    public Task Start(Guid deviceID, ResponsePlayerState state, ResponseQueue queue)
    {
        StartList.Add((deviceID, state, queue));
        return OnStart?.Invoke(deviceID, state, queue) ?? Task.CompletedTask;
    }
}
