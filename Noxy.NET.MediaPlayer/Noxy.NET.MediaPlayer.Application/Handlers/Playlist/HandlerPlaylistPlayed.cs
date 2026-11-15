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

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public class HandlerPlaylistPlayed(IUnitOfWorkFactory factoryUnitOfWork, IOptions<ConfigPlayback> configPlayback, IPublisher publisher, TimeProvider time) : ICommandHandler<CommandPlaylistPlayed>
{
    public async ValueTask<Unit> Handle(CommandPlaylistPlayed command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        PlaylistSummary summary = await uow.Playlist.FindPlaylistSummary(command.ID, command.UserID, cancellationToken) ?? throw new PlaylistNotFoundException();
        EntityPlaylist playlist = summary.Playlist;
        DateTime now = time.GetUtcNow().UtcDateTime;

        // A double click or two devices playing the same playlist count as one play.
        if (playlist.TimeLastPlayed is { } last && now - last < TimeSpan.FromSeconds(configPlayback.Value.PlayedWindowSeconds)) return Unit.Value;

        // The modified time stays as it is on purpose: a play is not a change to the playlist, so it never moves the entries ETag.
        playlist.TimeLastPlayed = now;
        uow.Playlist.UpdatePlaylist(playlist);
        await uow.Commit(cancellationToken);

        await publisher.Publish(new NotificationPlaylistUpdated(command.UserID, playlist.ToResponse(summary.EntryCount)), cancellationToken);
        return Unit.Value;
    }
}
