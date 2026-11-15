using EntityPlaylist = Noxy.NET.MediaPlayer.Domain.Entities.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Models.Playlist;

public record PlaylistSummary(EntityPlaylist Playlist, int EntryCount);
