using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;

[Table("Album")]
[Index(nameof(UserID), nameof(IdentityKey), IsUnique = true)]
public class TableAlbum : BaseTable
{
    [Required]
    public Guid UserID { get; set; }

    [ForeignKey(nameof(UserID))]
    public TableUser? User { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(1024)]
    public required string TitleKey { get; set; }

    [Required]
    [MaxLength(4096)]
    public required string IdentityKey { get; set; }

    [Required]
    public DateTime TimeModified { get; set; }

    public List<TableAlbumArtist> ArtistLinkList { get; set; } = [];
}
