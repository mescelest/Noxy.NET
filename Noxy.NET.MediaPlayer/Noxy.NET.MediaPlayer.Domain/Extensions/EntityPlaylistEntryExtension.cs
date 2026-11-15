using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Extensions;

public static class EntityPlaylistEntryExtension
{
    extension(EntityPlaylistEntry entry)
    {
        // An entry read together with its playlist always has its media file. One built without it is a bug in the caller, so say which entry.
        public EntityMediaFile GetLoadedMediaFile()
        {
            return entry.MediaFile ?? throw new InvalidOperationException($"The entry {entry.ID} was loaded without its media.");
        }
    }
}
