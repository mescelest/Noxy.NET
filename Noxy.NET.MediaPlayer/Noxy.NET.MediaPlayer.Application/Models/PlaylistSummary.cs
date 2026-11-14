using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Models;

public record PlaylistSummary(Playlist Playlist, int EntryCount);
