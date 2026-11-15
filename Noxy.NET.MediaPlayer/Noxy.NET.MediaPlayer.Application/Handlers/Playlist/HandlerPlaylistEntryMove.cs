using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using EntityPlaylist = Noxy.NET.MediaPlayer.Domain.Entities.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public class HandlerPlaylistEntryMove(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher, TimeProvider time) : HandlerPlaylistChange<CommandPlaylistEntryMove, ResponsePlaylistDetail>(factoryUnitOfWork, publisher, time)
{
    public override async ValueTask<ResponsePlaylistDetail> Handle(CommandPlaylistEntryMove command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = FactoryUnitOfWork.Create();

        EntityPlaylist playlist = await Load(uow, command.UserID, command.ID, cancellationToken);

        PlaylistEntry entry = playlist.EntryList.FirstOrDefault(x => x.ID == command.EntryID) ?? throw new PlaylistEntryNotFoundException();
        if (command.Position < 0 || command.Position >= playlist.EntryList.Count) throw new InvalidPlaylistPositionException();

        // A move to where the entry already is changes nothing, so it leaves the modified time and the ETag alone.
        if (playlist.EntryList.IndexOf(entry) == command.Position) return playlist.ToDetailResponse();

        playlist.EntryList.Remove(entry);
        playlist.EntryList.Insert(command.Position, entry);

        foreach (PlaylistEntry changed in playlist.RenumberEntryList())
        {
            uow.Playlist.UpdatePlaylistEntry(changed);
        }

        return await Save(uow, command.UserID, playlist, cancellationToken);
    }
}
