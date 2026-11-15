using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;

[Table("MediaFileGenre")]
[Index(nameof(MediaFileID), nameof(Position))]
[Index(nameof(GenreID))]
public class TableMediaFileGenre : BaseTable
{
    [Required]
    public Guid MediaFileID { get; set; }

    [ForeignKey(nameof(MediaFileID))]
    public TableMediaFile? MediaFile { get; set; }

    [Required]
    public Guid GenreID { get; set; }

    [ForeignKey(nameof(GenreID))]
    public TableGenre? Genre { get; set; }

    [Required]
    public int Position { get; set; }
}
