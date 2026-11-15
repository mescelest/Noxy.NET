using Microsoft.AspNetCore.SignalR;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeHubClients(FakeHubContext context) : IHubClients
{
    public IClientProxy All => throw new NotSupportedException();

    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds)
    {
        throw new NotSupportedException();
    }

    public IClientProxy Client(string connectionId)
    {
        throw new NotSupportedException();
    }

    public IClientProxy Clients(IReadOnlyList<string> connectionIds)
    {
        throw new NotSupportedException();
    }

    public IClientProxy Group(string groupName)
    {
        throw new NotSupportedException();
    }

    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds)
    {
        throw new NotSupportedException();
    }

    public IClientProxy Groups(IReadOnlyList<string> groupNames)
    {
        throw new NotSupportedException();
    }

    public IClientProxy User(string userId)
    {
        return new FakeClientProxy(context, userId);
    }

    public IClientProxy Users(IReadOnlyList<string> userIds)
    {
        throw new NotSupportedException();
    }
}
