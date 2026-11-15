using Mediator;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public class HandlerPlaylistCreate(IUnitOfWorkFactory factoryUnitOfWork, IOptions<ConfigPlaylist> configPlaylist, IPublisher publisher, TimeProvider time) : ICommandHandler<CommandPlaylistCreate, ResponsePlaylist>
{
    public async ValueTask<ResponsePlaylist> Handle(CommandPlaylistCreate command, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command.Name, nameof(command));

        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        if (await uow.Playlist.CountPlaylists(command.UserID, cancellationToken) >= configPlaylist.Value.MaxPlaylistsPerUser) throw new PlaylistLimitReachedException();

        DateTime now = time.GetUtcNow().UtcDateTime;
        EntityPlaylist playlist = new()
        {
            UserID = command.UserID,
            Name = command.Name.Trim(),
            AllowDuplicates = command.AllowDuplicates,
            TimeCreated = now,
            TimeModified = now
        };

        uow.Playlist.AddPlaylist(playlist);
        await uow.Commit(cancellationToken);

        ResponsePlaylist response = playlist.ToResponse(0);
        await publisher.Publish(new NotificationPlaylistUpdated(command.UserID, response), cancellationToken);
        return response;
    }
}
