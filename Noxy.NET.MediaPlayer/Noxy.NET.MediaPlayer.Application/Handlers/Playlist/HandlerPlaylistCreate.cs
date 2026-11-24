using Mediator;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;
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

        // The new playlist is on top; every other one of the user moves down by one, in the order they have.
        List<PlaylistSummary> existingList = await uow.Playlist.GetPlaylistSummaryList(command.UserID, cancellationToken);
        for (int index = 0; index < existingList.Count; index++)
        {
            existingList[index].Playlist.Position = index + 1;
            uow.Playlist.UpdatePlaylist(existingList[index].Playlist);
        }

        uow.Playlist.AddPlaylist(playlist);
        await uow.Commit(cancellationToken);

        ResponsePlaylist response = playlist.ToResponse(0);
        await publisher.Publish(new NotificationPlaylistUpdated(command.UserID, response), cancellationToken);

        // The other playlists moved down, which the update of the new one does not tell a device.
        if (existingList.Count > 0)
        {
            await publisher.Publish(new NotificationPlaylistOrderChanged(command.UserID), cancellationToken);
        }

        return response;
    }
}
