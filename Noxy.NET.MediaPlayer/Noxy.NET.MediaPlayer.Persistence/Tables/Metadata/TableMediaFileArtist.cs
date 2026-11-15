using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;

[Table("MediaFileArtist")]
[Index(nameof(MediaFileID), nameof(Position))]
[Index(nameof(ArtistID))]
public class TableMediaFileArtist : BaseTable
{
    [Required]
    public Guid MediaFileID { get; set; }

    [ForeignKey(nameof(MediaFileID))]
    public TableMediaFile? MediaFile { get; set; }

    [Required]
    public Guid ArtistID { get; set; }

    [ForeignKey(nameof(ArtistID))]
    public TableArtist? Artist { get; set; }

    [Required]
    public int Position { get; set; }
}
