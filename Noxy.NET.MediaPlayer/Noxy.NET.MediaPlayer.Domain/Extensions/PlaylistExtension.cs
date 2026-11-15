using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Domain.Extensions;

public static class PlaylistExtension
{
    extension(Playlist playlist)
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
                TimeModified = DateTime.SpecifyKind(playlist.TimeModified, DateTimeKind.Utc)
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
