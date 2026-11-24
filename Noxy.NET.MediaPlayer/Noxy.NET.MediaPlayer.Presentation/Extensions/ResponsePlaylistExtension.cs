using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class ResponsePlaylistExtension
{
    private const int HueCount = 360;

    extension(ResponsePlaylist playlist)
    {
        // The colour of the playlist's graphic on the colour wheel, in degrees: the one the user chose, else the default one.
        public int ArtHueCurrent => playlist.ArtHue ?? playlist.ArtHueDefault;

        // The colour a playlist has when none is chosen. It comes from the ID, so it stays the same when the playlist is renamed and from one start of the app to the next.
        public int ArtHueDefault => (int)((uint)playlist.ID.GetHashCode() % HueCount);
    }
}
