using Mediator;
using Noxy.NET.MediaPlayer.Application.Models;

namespace Noxy.NET.MediaPlayer.Application.Queries.Playlist;

public record QueryPlaylistList(Guid UserID, string? IfNoneMatch) : IQuery<PlaylistLibrary>;
