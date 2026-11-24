using Noxy.NET.MediaPlayer.Domain.Abstractions;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

public class ResponsePlaylist : BaseResponse
{
    public required Guid ID { get; init; }
    public required string Name { get; init; }
    public required bool AllowDuplicates { get; init; }

    // Where the playlist stands in the user's own order, 0 on top.
    public int Position { get; init; }
    public required int EntryCount { get; init; }
    public required DateTime TimeCreated { get; init; }
    public required DateTime TimeModified { get; init; }
    public required DateTime? TimeLastPlayed { get; init; }

    // The hue the user chose for the tile, 0 to 359. Null means the one made from the ID.
    public int? ArtHue { get; init; }
}
