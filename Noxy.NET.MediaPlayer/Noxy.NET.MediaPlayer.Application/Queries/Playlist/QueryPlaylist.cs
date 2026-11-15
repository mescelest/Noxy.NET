using Mediator;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Queries.Playlist;

public record QueryPlaylist(Guid UserID, Guid ID, string? IfNoneMatch) : IQuery<PlaylistView>;
