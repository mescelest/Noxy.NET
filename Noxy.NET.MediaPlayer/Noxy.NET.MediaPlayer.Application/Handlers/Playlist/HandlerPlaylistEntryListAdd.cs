using Mediator;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using EntityPlaylist = Noxy.NET.MediaPlayer.Domain.Entities.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public class HandlerPlaylistEntryListAdd(IUnitOfWorkFactory factoryUnitOfWork, IOptions<ConfigPlaylist> configPlaylist, IPublisher publisher, TimeProvider time)
    : HandlerPlaylistChange<CommandPlaylistEntryListAdd, ResponsePlaylistDetail>(factoryUnitOfWork, publisher, time)
{
    public override async ValueTask<ResponsePlaylistDetail> Handle(CommandPlaylistEntryListAdd command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = FactoryUnitOfWork.Create();

        EntityPlaylist playlist = await uow.LoadPlaylistWithEntries(command.UserID, command.ID, cancellationToken);
        List<PlaylistEntry> entryList = playlist.EntryList;

        int position = command.Position ?? entryList.Count;
        if (position < 0 || position > entryList.Count) throw new InvalidPlaylistPositionException();

        // Every media file must be the user's before anything changes.
        Dictionary<Guid, MediaFile> mediaFileMap = [];
        foreach (Guid mediaFileID in command.MediaFileIDList.Distinct())
        {
            mediaFileMap[mediaFileID] = await uow.Media.FindMediaFile(mediaFileID, command.UserID, cancellationToken) ?? throw new MediaNotFoundException();
        }

        HashSet<Guid> present = [.. entryList.Select(x => x.MediaFileID)];
        List<PlaylistEntry> addedList = [];
        foreach (Guid mediaFileID in command.MediaFileIDList)
        {
            // With duplicates off a media file already there, or repeated in this request, is skipped.
            if (!playlist.AllowDuplicates && !present.Add(mediaFileID)) continue;

            addedList.Add(new() { PlaylistID = playlist.ID, MediaFileID = mediaFileID, Position = 0, MediaFile = mediaFileMap[mediaFileID] });
        }

        if (addedList.Count == 0) return playlist.ToDetailResponse();

        if (entryList.Count + addedList.Count > configPlaylist.Value.MaxEntriesPerPlaylist) throw new PlaylistEntryLimitReachedException();

        entryList.InsertRange(position, addedList);
        List<PlaylistEntry> changedList = playlist.RenumberEntryList();

        HashSet<Guid> addedIDSet = [.. addedList.Select(x => x.ID)];
        foreach (PlaylistEntry entry in addedList)
        {
            uow.Playlist.AddPlaylistEntry(entry);
        }

        foreach (PlaylistEntry entry in changedList.Where(x => !addedIDSet.Contains(x.ID)))
        {
            uow.Playlist.UpdatePlaylistEntry(entry);
        }

        await Save(uow, command.UserID, playlist, cancellationToken);
        return playlist.ToDetailResponse();
    }
}
