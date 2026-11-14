using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Application.Configurations;

public class ConfigPlaylist
{
    public const string SectionName = "Playlist";

    [Range(1, 100_000)]
    public int MaxPlaylistsPerUser { get; set; } = 100;

    [Range(1, 100_000)]
    public int MaxEntriesPerPlaylist { get; set; } = 1000;
}
