using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class ResponsePlaylistListExtension
{
    extension(List<ResponsePlaylist> playlistList)
    {
        // The played ones first, newest played first, then the never played ones, newest modified first. A tie keeps the order of the input. The input list is not changed.
        public List<ResponsePlaylist> OrderMostRecentlyPlayedFirst()
        {
            List<ResponsePlaylist> playedList = [.. playlistList.Where(x => x.TimeLastPlayed is not null).OrderByDescending(x => x.TimeLastPlayed)];
            List<ResponsePlaylist> neverPlayedList = [.. playlistList.Where(x => x.TimeLastPlayed is null).OrderByDescending(x => x.TimeModified)];

            return [.. playedList, .. neverPlayedList];
        }

        // What a short list of recent playlists shows: the played ones, newest first; when none was ever played, the first of the same order. At most count.
        public List<ResponsePlaylist> TakeRecentlyPlayed(int count)
        {
            List<ResponsePlaylist> orderedList = playlistList.OrderMostRecentlyPlayedFirst();
            bool anyPlayed = orderedList.Any(x => x.TimeLastPlayed is not null);

            return [.. orderedList.Where(x => !anyPlayed || x.TimeLastPlayed is not null).Take(count)];
        }
    }
}
