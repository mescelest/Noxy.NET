using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using EntityPlaylist = Noxy.NET.MediaPlayer.Domain.Entities.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public class HandlerPlaylistDelete(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher) : ICommandHandler<CommandPlaylistDelete>
{
    public async ValueTask<Unit> Handle(CommandPlaylistDelete command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        EntityPlaylist playlist = await uow.Playlist.FindPlaylist(command.ID, command.UserID, cancellationToken) ?? throw new PlaylistNotFoundException();

        uow.Playlist.RemovePlaylist(playlist);
        await uow.Commit(cancellationToken);

        await publisher.Publish(new NotificationPlaylistDeleted(command.UserID, command.ID), cancellationToken);
        return Unit.Value;
    }
}
