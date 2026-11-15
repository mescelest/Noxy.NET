namespace Noxy.NET.MediaPlayer.Application.Models.Metadata;

public record MediaTagSet(
    string? Title,
    List<string> ArtistList,
    string? Album,
    List<string> AlbumArtistList,
    List<string> GenreList,
    int? TrackNumber,
    int? DiscNumber,
    int? Year,
    string? Comment,
    string? Subtitle,
    TimeSpan? Duration)
{
    // What a file with no readable tags gives.
    public static MediaTagSet Empty => new(null, [], null, [], [], null, null, null, null, null, null);
}
