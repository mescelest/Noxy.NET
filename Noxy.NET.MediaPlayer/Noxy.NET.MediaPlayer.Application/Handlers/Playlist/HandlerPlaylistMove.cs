using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public class HandlerPlaylistMove(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher) : ICommandHandler<CommandPlaylistMove, List<ResponsePlaylist>>
{
    // Moves the playlist to the position and renumbers the others, so the positions of the user stay 0 to n-1. The answer is the whole list in its new order. A move is not an edit of the playlist, so no modified time changes.
    public async ValueTask<List<ResponsePlaylist>> Handle(CommandPlaylistMove command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();
        List<PlaylistSummary> list = await uow.Playlist.GetPlaylistSummaryList(command.UserID, cancellationToken);
        PlaylistSummary moved = list.FirstOrDefault(x => x.Playlist.ID == command.ID) ?? throw new PlaylistNotFoundException();
        if (command.Position < 0 || command.Position >= list.Count) throw new InvalidPlaylistPositionException();

        // A move to where the playlist already is changes nothing.
        if (list.IndexOf(moved) == command.Position) return ToResponseList(list);

        list.Remove(moved);
        list.Insert(command.Position, moved);
        for (int index = 0; index < list.Count; index++)
        {
            if (list[index].Playlist.Position == index) continue;

            list[index].Playlist.Position = index;
            uow.Playlist.UpdatePlaylist(list[index].Playlist);
        }

        await uow.Commit(cancellationToken);
        await publisher.Publish(new NotificationPlaylistOrderChanged(command.UserID), cancellationToken);

        return ToResponseList(list);
    }

    private static List<ResponsePlaylist> ToResponseList(List<PlaylistSummary> list)
    {
        return [.. list.Select(x => x.Playlist.ToResponse(x.EntryCount))];
    }
}
