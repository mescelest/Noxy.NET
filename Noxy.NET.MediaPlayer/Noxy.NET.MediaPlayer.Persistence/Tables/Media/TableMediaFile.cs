using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Media;

[Table("MediaFile")]
[Index(nameof(FilePath), IsUnique = true)]
[Index(nameof(UserID))]
public class TableMediaFile : BaseTable
{
    [Required]
    [MaxLength(512)]
    public required string FilePath { get; set; }

    [Required]
    [MaxLength(127)]
    public required string ContentType { get; set; }

    [Required]
    public required long SizeBytes { get; set; }

    [Required]
    [MaxLength(64)]
    public required string ContentHash { get; set; }

    [Required]
    public DateTime TimeModified { get; set; }

    [Required]
    public Guid UserID { get; set; }

    [ForeignKey(nameof(UserID))]
    public TableUser? User { get; set; }

    public TableMediaFileMetadata? Metadata { get; set; }
}
