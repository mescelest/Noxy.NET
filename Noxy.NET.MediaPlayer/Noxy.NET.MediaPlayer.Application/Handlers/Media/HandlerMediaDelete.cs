using Mediator;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Commands.Media;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Media;

public class HandlerMediaDelete(IUnitOfWorkFactory factoryUnitOfWork, IFileStorageService serviceFileStorage, IPublisher publisher, TimeProvider time, ILogger<HandlerMediaDelete> logger) : ICommandHandler<CommandMediaDelete>
{
    // Removes the media file and its file. Throws MediaNotFoundException for an unknown media file or another user's media file.
    public async ValueTask<Unit> Handle(CommandMediaDelete command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        EntityMediaFile file = await uow.Media.FindMediaFile(command.ID, command.UserID, cancellationToken) ?? throw new MediaNotFoundException();

        // The media file leaves every playlist in the same commit, so no entry is ever left pointing at it.
        DateTime now = time.GetUtcNow().UtcDateTime;
        List<ResponsePlaylist> playlistUpdatedList = [];
        foreach (EntityPlaylist playlist in await uow.Playlist.GetPlaylistListByMediaFileID(command.UserID, command.ID, cancellationToken))
        {
            foreach (EntityPlaylistEntry removed in playlist.EntryList.Where(x => x.MediaFileID == command.ID))
            {
                uow.Playlist.RemovePlaylistEntry(removed);
            }

            playlist.EntryList.RemoveAll(x => x.MediaFileID == command.ID);
            foreach (EntityPlaylistEntry changed in playlist.RenumberEntryList())
            {
                uow.Playlist.UpdatePlaylistEntry(changed);
            }

            playlist.TimeModified = now;
            uow.Playlist.UpdatePlaylist(playlist);
            playlistUpdatedList.Add(playlist.ToResponse(playlist.EntryList.Count));
        }

        // The row goes first: a failure after it leaves an orphaned file, never a media file that points at nothing.
        uow.Media.RemoveMediaFile(file);
        await uow.Commit(cancellationToken);

        try
        {
            serviceFileStorage.Delete(file.FilePath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not delete the file of deleted media file {MediaFileID}", command.ID);
        }

        await publisher.Publish(new NotificationMediaDeleted(command.UserID, command.ID), cancellationToken);

        foreach (ResponsePlaylist updated in playlistUpdatedList)
        {
            await publisher.Publish(new NotificationPlaylistUpdated(command.UserID, updated), cancellationToken);
        }

        return Unit.Value;
    }
}
