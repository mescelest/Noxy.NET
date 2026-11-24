using Mediator;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Queries.Metadata;

// The answer is null when the album is unknown, is another user's, or has no picture.
public record QueryAlbumArt(Guid UserID, Guid AlbumID, string? IfNoneMatch) : IQuery<AlbumArtPicture?>;
