using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

[Table("ExternalLogin")]
[Index(nameof(Provider), nameof(ProviderKey), IsUnique = true)]
public class TableExternalLogin : BaseTable
{
    [Required]
    public required ExternalLoginProvider Provider { get; set; }

    [Required]
    [MaxLength(128)]
    public required string ProviderKey { get; set; }

    [Required]
    public Guid UserID { get; set; }

    [ForeignKey(nameof(UserID))]
    public TableUser? User { get; set; }
}
