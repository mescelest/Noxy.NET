using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Models.Metadata;

public record ResolvedMetadata(List<EntityArtist> ArtistList, EntityAlbum? Album, List<EntityGenre> GenreList);
