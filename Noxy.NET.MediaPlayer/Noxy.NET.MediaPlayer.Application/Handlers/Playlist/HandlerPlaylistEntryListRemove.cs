using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using EntityPlaylist = Noxy.NET.MediaPlayer.Domain.Entities.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public class HandlerPlaylistEntryListRemove(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher, TimeProvider time) : HandlerPlaylistChange<CommandPlaylistEntryListRemove, ResponsePlaylistDetail>(factoryUnitOfWork, publisher, time)
{
    public override async ValueTask<ResponsePlaylistDetail> Handle(CommandPlaylistEntryListRemove command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = FactoryUnitOfWork.Create();

        EntityPlaylist playlist = await Load(uow, command.UserID, command.ID, cancellationToken);

        HashSet<Guid> removeIDSet = [.. command.EntryIDList];
        List<PlaylistEntry> removedList = [.. playlist.EntryList.Where(x => removeIDSet.Contains(x.ID))];
        if (removedList.Count != removeIDSet.Count) throw new PlaylistEntryNotFoundException();

        playlist.EntryList.RemoveAll(x => removeIDSet.Contains(x.ID));
        List<PlaylistEntry> changedList = playlist.RenumberEntryList();

        foreach (PlaylistEntry entry in removedList)
        {
            uow.Playlist.RemovePlaylistEntry(entry);
        }

        foreach (PlaylistEntry entry in changedList)
        {
            uow.Playlist.UpdatePlaylistEntry(entry);
        }

        return await Save(uow, command.UserID, playlist, cancellationToken);
    }
}
