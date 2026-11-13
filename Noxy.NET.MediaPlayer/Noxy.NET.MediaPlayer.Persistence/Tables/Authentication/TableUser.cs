using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

[Table("User")]
[Index(nameof(Email), IsUnique = true)]
public class TableUser : BaseTable
{
    [Required]
    [MaxLength(256)]
    public required string Email { get; set; }

    public bool IsEmailVerified { get; set; }

    [MaxLength(512)]
    public string? PasswordHash { get; set; }

    public List<TableExternalLogin> ExternalLoginList { get; set; } = [];
}
