using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Presentation.Features.MetadataList;

public record ActionMetadataListLoaded(List<ResponseArtist> ArtistList, List<ResponseAlbum> AlbumList, List<ResponseGenre> GenreList);
