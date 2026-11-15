namespace Noxy.NET.MediaPlayer.Application.Models.Playlist;

public record PlaylistStamp(Guid ID, DateTime TimeModified, DateTime? TimeLastPlayed = null);
