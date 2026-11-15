using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using EntityPlaylist = Noxy.NET.MediaPlayer.Domain.Entities.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public abstract class HandlerPlaylistChange<TCommand, TResponse>(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher, TimeProvider time) : ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    protected IUnitOfWorkFactory FactoryUnitOfWork { get; } = factoryUnitOfWork;

    public abstract ValueTask<TResponse> Handle(TCommand command, CancellationToken cancellationToken);

    // The entries are in position order, which is the order every change works on.
    protected static async Task<EntityPlaylist> Load(IUnitOfWork uow, Guid userID, Guid id, CancellationToken cancellationToken)
    {
        EntityPlaylist playlist = await uow.Playlist.FindPlaylistWithEntries(id, userID, cancellationToken) ?? throw new PlaylistNotFoundException();
        playlist.EntryList = [.. playlist.EntryList.OrderBy(x => x.Position)];
        return playlist;
    }

    // Every change bumps the modified time in the same commit, which is what keeps the ETags correct.
    protected async Task<ResponsePlaylistDetail> Save(IUnitOfWork uow, Guid userID, EntityPlaylist playlist, CancellationToken cancellationToken)
    {
        playlist.TimeModified = time.GetUtcNow().UtcDateTime;
        uow.Playlist.UpdatePlaylist(playlist);
        await uow.Commit(cancellationToken);

        ResponsePlaylist pushed = playlist.ToResponse(playlist.EntryList.Count);
        await publisher.Publish(new NotificationPlaylistUpdated(userID, pushed), cancellationToken);
        return playlist.ToDetailResponse();
    }
}
