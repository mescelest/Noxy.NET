using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions;

public abstract class BaseHandlerPlaylistChange<TCommand, TResponse>(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher, TimeProvider time) : ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    protected IUnitOfWorkFactory FactoryUnitOfWork { get; } = factoryUnitOfWork;

    public abstract ValueTask<TResponse> Handle(TCommand command, CancellationToken cancellationToken);

    // Every change bumps the modified time in the same commit, which is what keeps the ETags correct.
    protected async Task Save(IUnitOfWork uow, Guid userID, EntityPlaylist playlist, CancellationToken cancellationToken)
    {
        playlist.TimeModified = time.GetUtcNow().UtcDateTime;
        uow.Playlist.UpdatePlaylist(playlist);
        await uow.Commit(cancellationToken);

        ResponsePlaylist pushed = playlist.ToResponse(playlist.EntryList.Count);
        await publisher.Publish(new NotificationPlaylistUpdated(userID, pushed), cancellationToken);
    }
}
