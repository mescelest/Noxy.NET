using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Interfaces.Notifiers;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.API.Notifiers;

public class SignalRPlaylistNotifier(IHubContext<MediaHub> hub) : IPlaylistNotifier
{
    // A user with no live connection simply receives nothing: their next start-up diff covers what they missed.
    public async Task Updated(Guid userID, ResponsePlaylist playlist, CancellationToken cancellationToken = default)
    {
        await hub.Clients.User(userID.ToString()).SendAsync(nameof(MediaHubEvent.PlaylistUpdated), playlist, cancellationToken);
    }

    public async Task Deleted(Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        await hub.Clients.User(userID.ToString()).SendAsync(nameof(MediaHubEvent.PlaylistDeleted), id, cancellationToken);
    }
}
