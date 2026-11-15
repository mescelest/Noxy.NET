using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Playback;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Notifications.Playback;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playback;

public class HandlerPlaybackSessionClear(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher) : ICommandHandler<CommandPlaybackSessionClear>
{
    // Clearing a session that does not exist is not an error: nothing is saved and nothing is published.
    public async ValueTask<Unit> Handle(CommandPlaybackSessionClear command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        EntityPlaybackSession? session = await uow.Playback.FindSession(command.UserID, cancellationToken);
        if (session is null) return Unit.Value;

        uow.Playback.RemoveSession(session);
        await uow.Commit(cancellationToken);

        await publisher.Publish(new NotificationPlaybackSessionCleared(command.UserID), cancellationToken);
        return Unit.Value;
    }
}
