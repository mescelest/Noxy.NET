using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Models.Playlist;

public record PlaylistSummary(EntityPlaylist Playlist, int EntryCount);
