using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakePlaybackRepository(FakeStore store) : IPlaybackRepository
{
    private readonly List<EntityPlaybackSession> _pendingAddList = [];
    private readonly List<EntityPlaybackSession> _pendingUpdateList = [];
    private readonly List<EntityPlaybackSession> _pendingRemovalList = [];

    public void Flush()
    {
        // The unique index: one session per user.
        foreach (EntityPlaybackSession added in _pendingAddList)
        {
            if (store.PlaybackSessionList.Any(x => x.UserID == added.UserID)) throw new MetadataKeyConflictException();

            store.PlaybackSessionList.Add(added);
        }

        _pendingAddList.Clear();

        foreach (EntityPlaybackSession updated in _pendingUpdateList)
        {
            int index = store.PlaybackSessionList.FindIndex(x => x.ID == updated.ID);
            if (index >= 0) store.PlaybackSessionList[index] = updated;
        }

        _pendingUpdateList.Clear();

        foreach (EntityPlaybackSession removed in _pendingRemovalList)
        {
            store.PlaybackSessionList.RemoveAll(x => x.ID == removed.ID);
        }

        _pendingRemovalList.Clear();
    }

    public Task<EntityPlaybackSession?> FindSession(Guid userID, CancellationToken cancellationToken = default)
    {
        EntityPlaybackSession? found = store.PlaybackSessionList.FirstOrDefault(x => x.UserID == userID);

        return Task.FromResult(found is null ? null : Copy(found));
    }

    public void AddSession(EntityPlaybackSession session)
    {
        _pendingAddList.Add(Copy(session));
    }

    public void UpdateSession(EntityPlaybackSession session)
    {
        _pendingUpdateList.Add(Copy(session));
    }

    public void RemoveSession(EntityPlaybackSession session)
    {
        _pendingRemovalList.Add(Copy(session));
    }

    private static EntityPlaybackSession Copy(EntityPlaybackSession session)
    {
        return new()
        {
            ID = session.ID,
            TimeCreated = session.TimeCreated,
            UserID = session.UserID,
            QueueMediaFileIDList = [.. session.QueueMediaFileIDList],
            CurrentIndex = session.CurrentIndex,
            PositionSeconds = session.PositionSeconds,
            PlaylistID = session.PlaylistID,
            TimeModified = session.TimeModified
        };
    }
}
