using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityAlbum : BaseEntity
{
    public required Guid UserID { get; set; }

    public required string Title { get; set; }
    public required string TitleKey { get; set; }

    // The title key, a pipe, then the artist IDs sorted ascending and joined with commas.
    public required string IdentityKey { get; set; }

    public DateTime TimeModified { get; set; } = DateTime.UtcNow;

    // In position order.
    public List<EntityAlbumArtist> ArtistLinkList { get; set; } = [];

    // The title key, a pipe, then the artist IDs sorted ascending and joined with commas: two albums are the same when their title and set of artists are the same.
    public static string BuildIdentityKey(string titleKey, IEnumerable<Guid> artistIDList)
    {
        return $"{titleKey}|{string.Join(',', artistIDList.Distinct().Order())}";
    }
}
