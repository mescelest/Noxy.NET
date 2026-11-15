using Mediator;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Commands.Playback;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Notifications.Playback;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playback;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playback;

public class HandlerPlaybackSessionSave(IUnitOfWorkFactory factoryUnitOfWork, IOptions<ConfigPlayback> configPlayback, IPublisher publisher, TimeProvider time) : ICommandHandler<CommandPlaybackSessionSave, ResponsePlaybackSession>
{
    public const double PositionToleranceSeconds = 0.5;

    // Replaces the whole session, so the last save wins. A save that changes nothing is not committed or published.
    // Throws InvalidPlaybackSessionException for a bad queue, index or position, and MediaNotFoundException or PlaylistNotFoundException for one the user does not own.
    public async ValueTask<ResponsePlaybackSession> Handle(CommandPlaybackSessionSave command, CancellationToken cancellationToken)
    {
        Validate(command);

        // Another device may create the first session at the same moment: then the commit conflicts and this runs again as an update.
        (ResponsePlaybackSession response, bool changed) = await factoryUnitOfWork.RunWithKeyConflictRetry(uow => Apply(uow, command, cancellationToken), cancellationToken);

        if (changed) await publisher.Publish(new NotificationPlaybackSessionUpdated(command.UserID, response), cancellationToken);
        return response;
    }

    private void Validate(CommandPlaybackSessionSave command)
    {
        int maxQueueLength = configPlayback.Value.MaxQueueLength;
        if (command.QueueMediaFileIDList.Count > maxQueueLength) throw new InvalidPlaybackSessionException($"The queue is longer than {maxQueueLength} media files.");

        if (command.CurrentIndex is { } index && (index < 0 || index >= command.QueueMediaFileIDList.Count)) throw new InvalidPlaybackSessionException("The current index is outside the queue.");

        if (!double.IsFinite(command.PositionSeconds) || command.PositionSeconds < 0) throw new InvalidPlaybackSessionException("The position must be a number of seconds that is not negative.");
    }

    private async Task<(ResponsePlaybackSession Response, bool Changed)> Apply(IUnitOfWork uow, CommandPlaybackSessionSave command, CancellationToken cancellationToken)
    {
        List<Guid> distinctList = [.. command.QueueMediaFileIDList.Distinct()];
        if (distinctList.Count > 0 && (await uow.Media.GetOwnedMediaFileIDList(command.UserID, distinctList, cancellationToken)).Distinct().Count() != distinctList.Count) throw new MediaNotFoundException();

        if (command.PlaylistID is { } playlistID && await uow.Playlist.FindPlaylist(playlistID, command.UserID, cancellationToken) is null) throw new PlaylistNotFoundException();

        EntityPlaybackSession? session = await uow.Playback.FindSession(command.UserID, cancellationToken);
        DateTime now = time.GetUtcNow().UtcDateTime;

        if (session is null)
        {
            session = new() { UserID = command.UserID, TimeModified = now };
            Fill(session, command);
            uow.Playback.AddSession(session);
        }
        else
        {
            if (IsSame(session, command)) return (session.ToResponse(), false);

            Fill(session, command);
            session.SetModified(now);
            uow.Playback.UpdateSession(session);
        }

        await uow.Commit(cancellationToken);
        return (session.ToResponse(), true);
    }

    private static bool IsSame(EntityPlaybackSession session, CommandPlaybackSessionSave command)
    {
        return session.QueueMediaFileIDList.SequenceEqual(command.QueueMediaFileIDList) && session.CurrentIndex == command.CurrentIndex && IsSamePosition(session.PositionSeconds, command.PositionSeconds) && session.PlaylistID == command.PlaylistID;
    }

    // A position is a floating point number from a player, so two saves of the same moment differ by a fraction. A move smaller than the tolerance is not worth saving or pushing.
    private static bool IsSamePosition(double stored, double saved)
    {
        return Math.Abs(stored - saved) <= PositionToleranceSeconds;
    }

    private static void Fill(EntityPlaybackSession session, CommandPlaybackSessionSave command)
    {
        session.QueueMediaFileIDList = [.. command.QueueMediaFileIDList];
        session.CurrentIndex = command.CurrentIndex;
        session.PositionSeconds = command.PositionSeconds;
        session.PlaylistID = command.PlaylistID;
    }
}
