using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Media;

[Table("MediaFileMetadata")]
[Index(nameof(MediaFileID), IsUnique = true)]
public class TableMediaFileMetadata : BaseTable
{
    [Required]
    [MaxLength(256)]
    public required string Title { get; set; }

    [MaxLength(256)]
    public string? Subtitle { get; set; }

    [MaxLength(2048)]
    public string? Comment { get; set; }

    public int? TrackNumber { get; set; }
    public int? DiscNumber { get; set; }
    public int? Year { get; set; }
    public TimeSpan? Duration { get; set; }

    [Required]
    public Guid MediaFileID { get; set; }

    [ForeignKey(nameof(MediaFileID))]
    public TableMediaFile? MediaFile { get; set; }
}
