using Fluxor;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Presentation.Features.MetadataList;

[FeatureState]
public record FeatureMetadataListState
{
    public List<ResponseArtist> ArtistList { get; init; } = [];
    public List<ResponseAlbum> AlbumList { get; init; } = [];
    public List<ResponseGenre> GenreList { get; init; } = [];
}
