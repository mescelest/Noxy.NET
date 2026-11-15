using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Hubs;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeHubContext : IHubContext<MediaHub>
{
    public FakeHubContext()
    {
        Clients = new FakeHubClients(this);
    }

    public List<(string UserID, string Method, object?[] ArgumentList, CancellationToken Token)> SendList { get; } = [];
    public Exception? Failure { get; set; }

    public IHubClients Clients { get; }

    public IGroupManager Groups => throw new NotSupportedException();
}
