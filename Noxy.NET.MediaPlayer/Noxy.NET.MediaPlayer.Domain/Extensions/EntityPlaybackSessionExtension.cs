using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Playback;

namespace Noxy.NET.MediaPlayer.Domain.Extensions;

public static class EntityPlaybackSessionExtension
{
    extension(EntityPlaybackSession session)
    {
        // Takes a media file out of the queue everywhere it is, and returns whether it was there. The current index follows the media file that was playing, or is cleared with the position when that media file itself is gone.
        public bool RemoveMediaFile(Guid mediaFileID)
        {
            if (!session.QueueMediaFileIDList.Contains(mediaFileID)) return false;

            int? currentIndex = session.CurrentIndex;
            bool currentRemoved = currentIndex is { } index && session.QueueMediaFileIDList[index] == mediaFileID;
            int removedBefore = currentIndex is { } before ? session.QueueMediaFileIDList.Take(before).Count(x => x == mediaFileID) : 0;

            session.QueueMediaFileIDList.RemoveAll(x => x == mediaFileID);

            if (currentIndex is null) return true;

            if (currentRemoved)
            {
                session.CurrentIndex = null;
                session.PositionSeconds = 0;
            }
            else
            {
                session.CurrentIndex = currentIndex - removedBefore;
            }

            return true;
        }

        // Always later than the time it replaces, so two changes in the same clock tick still move the ETag.
        public void SetModified(DateTime now)
        {
            session.TimeModified = now > session.TimeModified ? now : session.TimeModified.AddTicks(1);
        }

        public ResponsePlaybackSession ToResponse()
        {
            return new()
            {
                ID = session.ID,
                QueueMediaFileIDList = [.. session.QueueMediaFileIDList],
                CurrentIndex = session.CurrentIndex,
                PositionSeconds = session.PositionSeconds,
                PlaylistID = session.PlaylistID,
                // Stored as UTC, but the database hands it back without a kind, which would serialize without a "Z".
                TimeModified = DateTime.SpecifyKind(session.TimeModified, DateTimeKind.Utc)
            };
        }
    }
}
