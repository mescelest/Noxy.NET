using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IPlaybackRepository
{
    // The user's one session, or null when there is none.
    Task<EntityPlaybackSession?> FindSession(Guid userID, CancellationToken cancellationToken = default);

    // A second session for the same user is rejected by the commit.
    void AddSession(EntityPlaybackSession session);

    void UpdateSession(EntityPlaybackSession session);

    void RemoveSession(EntityPlaybackSession session);
}
