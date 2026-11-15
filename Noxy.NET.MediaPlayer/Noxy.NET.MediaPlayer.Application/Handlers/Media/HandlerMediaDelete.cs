using Mediator;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Commands.Media;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Playback;
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
        DateTime now = time.GetUtcNow().UtcDateTime;

        // The media file leaves every playlist and the saved playback session in the same commit, so nothing is ever left pointing at it.
        List<ResponsePlaylist> playlistUpdatedList = await RemoveFromPlaylists(uow, command, now, cancellationToken);
        INotification? sessionNotification = await RemoveFromSession(uow, command, now, cancellationToken);

        // The row goes first: a failure after it leaves an orphaned file, never a media file that points at nothing.
        uow.Media.RemoveMediaFile(file);
        await uow.Commit(cancellationToken);

        DeleteFile(file);

        await publisher.Publish(new NotificationMediaDeleted(command.UserID, command.ID), cancellationToken);

        foreach (ResponsePlaylist updated in playlistUpdatedList)
        {
            await publisher.Publish(new NotificationPlaylistUpdated(command.UserID, updated), cancellationToken);
        }

        if (sessionNotification is not null) await publisher.Publish(sessionNotification, cancellationToken);

        return Unit.Value;
    }

    // Takes every entry of the media file out of the user's playlists, closes the gaps in their positions, and returns the changed playlists as they will be pushed.
    private static async Task<List<ResponsePlaylist>> RemoveFromPlaylists(IUnitOfWork uow, CommandMediaDelete command, DateTime now, CancellationToken cancellationToken)
    {
        List<ResponsePlaylist> updatedList = [];
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
            updatedList.Add(playlist.ToResponse(playlist.EntryList.Count));
        }

        return updatedList;
    }

    // Takes the media file out of the saved playback session, which goes away when nothing is left in its queue. Returns what to push: the updated session, the cleared session, or null when the session did not hold the media file.
    private static async Task<INotification?> RemoveFromSession(IUnitOfWork uow, CommandMediaDelete command, DateTime now, CancellationToken cancellationToken)
    {
        EntityPlaybackSession? session = await uow.Playback.FindSession(command.UserID, cancellationToken);
        if (session is null || !session.RemoveMediaFile(command.ID)) return null;

        if (session.QueueMediaFileIDList.Count == 0)
        {
            uow.Playback.RemoveSession(session);
            return new NotificationPlaybackSessionCleared(command.UserID);
        }

        session.SetModified(now);
        uow.Playback.UpdateSession(session);
        return new NotificationPlaybackSessionUpdated(command.UserID, session.ToResponse());
    }

    // A file that cannot be deleted is logged and left behind: the media file is already gone, so the delete still counts as done.
    private void DeleteFile(EntityMediaFile file)
    {
        try
        {
            serviceFileStorage.Delete(file.FilePath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not delete the file of deleted media file {MediaFileID}", file.ID);
        }
    }
}
