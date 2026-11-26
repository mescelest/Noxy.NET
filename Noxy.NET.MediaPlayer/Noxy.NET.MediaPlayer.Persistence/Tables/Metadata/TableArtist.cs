using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;

[Table("Artist")]
[Index(nameof(UserID), nameof(NameKey), IsUnique = true)]
public class TableArtist : BaseTable
{
    [Required]
    public Guid UserID { get; set; }

    [ForeignKey(nameof(UserID))]
    public TableUser? User { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(1024)]
    public required string NameKey { get; set; }

    [Required]
    public int Weight { get; set; } = MetadataWeight.Default;

    [Required]
    public DateTime TimeModified { get; set; }
}
