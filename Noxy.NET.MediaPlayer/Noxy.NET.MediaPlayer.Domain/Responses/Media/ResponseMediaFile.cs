using Noxy.NET.MediaPlayer.Domain.Abstractions;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Media;

public class ResponseMediaFile : BaseResponse
{
    public required Guid ID { get; init; }
    public required string Title { get; init; }
    public required string ContentType { get; init; }
    public required long SizeBytes { get; init; }
    public required DateTime TimeCreated { get; init; }
    public required DateTime TimeModified { get; init; }

    public string? Artist { get; init; }
    public string? Album { get; init; }
    public string? AlbumArtist { get; init; }
    public string? Subtitle { get; init; }
    public string? Comment { get; init; }
    public string? Genre { get; init; }
    public int? TrackNumber { get; init; }
    public int? DiscNumber { get; init; }
    public int? Year { get; init; }
    public TimeSpan? Duration { get; init; }

    public List<Guid> ArtistIDList { get; init; } = [];
    public Guid? AlbumID { get; init; }
    public List<Guid> AlbumArtistIDList { get; init; } = [];
    public List<Guid> GenreIDList { get; init; } = [];
}
