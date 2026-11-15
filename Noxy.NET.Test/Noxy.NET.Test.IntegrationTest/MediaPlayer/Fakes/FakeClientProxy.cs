using Microsoft.AspNetCore.SignalR;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeClientProxy(FakeHubContext context, string userID) : IClientProxy
{
    public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
    {
        if (context.Failure is not null) throw context.Failure;

        context.SendList.Add((userID, method, args, cancellationToken));
        return Task.CompletedTask;
    }
}
