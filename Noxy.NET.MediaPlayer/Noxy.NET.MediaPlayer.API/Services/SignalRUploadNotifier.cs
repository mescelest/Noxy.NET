using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.API.Services;

public class SignalRUploadNotifier(IHubContext<MediaHub> hub) : IUploadNotifier
{
    public async Task Notify(Guid userID, ResponseUploadResult result, CancellationToken cancellationToken = default)
    {
        // A user with no live connection simply receives nothing: the library list covers what they missed.
        await hub.Clients.User(userID.ToString()).SendAsync(result.Succeeded ? nameof(MediaHubEvent.UploadSucceeded) : nameof(MediaHubEvent.UploadFailed), result, cancellationToken);
    }
}
