using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public class HandlerPlaylistEntryListRemove(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher, TimeProvider time) : BaseHandlerPlaylistChange<CommandPlaylistEntryListRemove, ResponsePlaylistDetail>(factoryUnitOfWork, publisher, time)
{
    public override async ValueTask<ResponsePlaylistDetail> Handle(CommandPlaylistEntryListRemove command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = FactoryUnitOfWork.Create();

        EntityPlaylist playlist = await uow.LoadPlaylistWithEntries(command.UserID, command.ID, cancellationToken);

        HashSet<Guid> removeIDSet = [.. command.EntryIDList];
        List<EntityPlaylistEntry> removedList = [.. playlist.EntryList.Where(x => removeIDSet.Contains(x.ID))];
        if (removedList.Count != removeIDSet.Count) throw new PlaylistEntryNotFoundException();

        playlist.EntryList.RemoveAll(x => removeIDSet.Contains(x.ID));
        List<EntityPlaylistEntry> changedList = playlist.RenumberEntryList();

        foreach (EntityPlaylistEntry entry in removedList)
        {
            uow.Playlist.RemovePlaylistEntry(entry);
        }

        foreach (EntityPlaylistEntry entry in changedList)
        {
            uow.Playlist.UpdatePlaylistEntry(entry);
        }

        await Save(uow, command.UserID, playlist, cancellationToken);
        return playlist.ToDetailResponse();
    }
}
