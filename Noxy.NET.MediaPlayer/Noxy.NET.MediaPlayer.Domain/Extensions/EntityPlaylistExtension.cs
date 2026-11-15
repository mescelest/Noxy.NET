using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Domain.Extensions;

public static class EntityPlaylistExtension
{
    extension(EntityPlaylist playlist)
    {
        public ResponsePlaylist ToResponse(int entryCount)
        {
            return new()
            {
                ID = playlist.ID,
                Name = playlist.Name,
                AllowDuplicates = playlist.AllowDuplicates,
                EntryCount = entryCount,
                // Stored as UTC, but the database hands it back without a kind, which would serialize without a "Z".
                TimeCreated = DateTime.SpecifyKind(playlist.TimeCreated, DateTimeKind.Utc),
                TimeModified = DateTime.SpecifyKind(playlist.TimeModified, DateTimeKind.Utc),
                TimeLastPlayed = playlist.TimeLastPlayed is { } played ? DateTime.SpecifyKind(played, DateTimeKind.Utc) : null
            };
        }

        public ResponsePlaylistDetail ToDetailResponse()
        {
            return new()
            {
                ID = playlist.ID,
                Name = playlist.Name,
                AllowDuplicates = playlist.AllowDuplicates,
                EntryCount = playlist.EntryList.Count,
                TimeCreated = DateTime.SpecifyKind(playlist.TimeCreated, DateTimeKind.Utc),
                TimeModified = DateTime.SpecifyKind(playlist.TimeModified, DateTimeKind.Utc),
                TimeLastPlayed = playlist.TimeLastPlayed is { } played ? DateTime.SpecifyKind(played, DateTimeKind.Utc) : null,
                EntryList =
                [
                    .. playlist.EntryList.OrderBy(x => x.Position).Select(x => new ResponsePlaylistEntry
                    {
                        ID = x.ID,
                        MediaFileID = x.MediaFileID,
                        Position = x.Position,
                        MediaFile = x.GetLoadedMediaFile().ToResponse()
                    })
                ]
            };
        }
    }
}
