using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Models.Metadata;

public record MediaFileItemSet(List<EntityArtist> ArtistList, List<EntityGenre> GenreList, EntityAlbum? Album)
{
    // What a file that is not in the library yet has.
    public static MediaFileItemSet Empty => new([], [], null);

    // The items of the file as they are now. The file must have been loaded with its items.
    public static MediaFileItemSet Of(EntityMediaFile file)
    {
        return new(
            [.. file.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.Artist ?? throw new InvalidOperationException($"The media file {file.ID} was loaded without its artists."))],
            [.. file.GenreLinkList.OrderBy(x => x.Position).Select(x => x.Genre ?? throw new InvalidOperationException($"The media file {file.ID} was loaded without its genres."))],
            file.Album);
    }
}
