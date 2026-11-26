namespace Noxy.NET.MediaPlayer.Domain.Models;

public record MediaTagValues(
    string Title,
    List<string> ArtistNameList,
    string? Album,
    List<string> AlbumArtistNameList,
    string? Subtitle,
    string? Comment,
    List<string> GenreNameList,
    int? TrackNumber,
    int? DiscNumber,
    int? Year);
