using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;

[Table("AlbumArtist")]
[Index(nameof(AlbumID), nameof(Position))]
[Index(nameof(ArtistID))]
public class TableAlbumArtist : BaseTable
{
    [Required]
    public Guid AlbumID { get; set; }

    [ForeignKey(nameof(AlbumID))]
    public TableAlbum? Album { get; set; }

    [Required]
    public Guid ArtistID { get; set; }

    [ForeignKey(nameof(ArtistID))]
    public TableArtist? Artist { get; set; }

    [Required]
    public int Position { get; set; }
}
