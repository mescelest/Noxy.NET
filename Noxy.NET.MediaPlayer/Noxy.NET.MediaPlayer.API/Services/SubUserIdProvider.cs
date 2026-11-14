using Microsoft.AspNetCore.SignalR;

namespace Noxy.NET.MediaPlayer.API.Services;

public class SubUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User.FindFirst("sub")?.Value;
    }
}
