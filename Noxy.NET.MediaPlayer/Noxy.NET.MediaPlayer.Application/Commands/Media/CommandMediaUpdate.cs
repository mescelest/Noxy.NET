using Mediator;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Commands.Media;

// A member left at its default was not sent, which is different from a member sent as null. For each of artist, album, album artist and genre either the text or the IDs may be sent, never both.
public record CommandMediaUpdate(
    Guid UserID,
    Guid ID,
    Optional<string?> Title = default,
    Optional<string?> Artist = default,
    Optional<string?> Album = default,
    Optional<string?> AlbumArtist = default,
    Optional<string?> Subtitle = default,
    Optional<string?> Comment = default,
    Optional<string?> Genre = default,
    Optional<int?> TrackNumber = default,
    Optional<int?> DiscNumber = default,
    Optional<int?> Year = default,
    Optional<List<Guid>?> ArtistIDList = default,
    Optional<Guid?> AlbumID = default,
    Optional<List<Guid>?> AlbumArtistIDList = default,
    Optional<List<Guid>?> GenreIDList = default) : ICommand<ResponseMediaFile>;
