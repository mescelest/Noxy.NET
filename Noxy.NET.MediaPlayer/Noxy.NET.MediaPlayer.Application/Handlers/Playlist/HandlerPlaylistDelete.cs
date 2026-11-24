using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public class HandlerPlaylistDelete(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher) : ICommandHandler<CommandPlaylistDelete>
{
    public async ValueTask<Unit> Handle(CommandPlaylistDelete command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        EntityPlaylist playlist = await uow.Playlist.FindPlaylist(command.ID, command.UserID, cancellationToken) ?? throw new PlaylistNotFoundException();

        uow.Playlist.RemovePlaylist(playlist);

        // The ones below it move up, so the positions of the user stay 0 to n-1.
        List<PlaylistSummary> remainingList = [.. (await uow.Playlist.GetPlaylistSummaryList(command.UserID, cancellationToken)).Where(x => x.Playlist.ID != command.ID)];
        for (int index = 0; index < remainingList.Count; index++)
        {
            if (remainingList[index].Playlist.Position == index) continue;

            remainingList[index].Playlist.Position = index;
            uow.Playlist.UpdatePlaylist(remainingList[index].Playlist);
        }

        await uow.Commit(cancellationToken);

        await publisher.Publish(new NotificationPlaylistDeleted(command.UserID, command.ID), cancellationToken);
        return Unit.Value;
    }
}
