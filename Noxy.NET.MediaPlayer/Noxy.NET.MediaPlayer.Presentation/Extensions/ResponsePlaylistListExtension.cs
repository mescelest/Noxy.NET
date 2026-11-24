using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class ResponsePlaylistListExtension
{
    extension(List<ResponsePlaylist> playlistList)
    {
        // The newest first, by the later of when a playlist was last played and when it was created, so a new playlist is on top until another is played. A tie keeps the order of the input. The input list is not changed.
        public List<ResponsePlaylist> OrderMostRecentFirst()
        {
            return [.. playlistList.OrderByDescending(x => x.TimeLastPlayed is { } played && played > x.TimeCreated ? played : x.TimeCreated)];
        }

        // The user's own order, the one the server saves: position 0 first. A tie keeps the order of the input. The input list is not changed.
        public List<ResponsePlaylist> OrderByPosition()
        {
            return [.. playlistList.OrderBy(x => x.Position)];
        }

        // The playlists whose name contains the text, ignoring case, in the order of the list; no text keeps them all.
        public List<ResponsePlaylist> FilterByName(string? search)
        {
            string text = search?.Trim() ?? string.Empty;

            return text.Length == 0 ? [.. playlistList] : [.. playlistList.Where(x => x.Name.Contains(text, StringComparison.OrdinalIgnoreCase))];
        }

        // What a short list of recent playlists shows: the played ones, newest first; when none was ever played, the first of the same order. At most count.
        public List<ResponsePlaylist> TakeRecentlyPlayed(int count)
        {
            List<ResponsePlaylist> playedList = [.. playlistList.Where(x => x.TimeLastPlayed is not null).OrderByDescending(x => x.TimeLastPlayed)];

            return [.. (playedList.Count > 0 ? playedList : playlistList.OrderMostRecentFirst()).Take(count)];
        }
    }
}
