using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

[Table("RefreshToken")]
[Index(nameof(TokenHash), IsUnique = true)]
[Index(nameof(UserID))]
public class TableRefreshToken : BaseTable
{
    [Required]
    [MaxLength(64)]
    public required string TokenHash { get; set; }

    [Required]
    public required DateTime TimeExpires { get; set; }

    public DateTime? TimeRevoked { get; set; }

    [Required]
    public Guid UserID { get; set; }

    [ForeignKey(nameof(UserID))]
    public TableUser? User { get; set; }
}
