using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Setting;

[Table(TableName)]
[Index(nameof(UserID), nameof(Key), IsUnique = true)]
public abstract class TableUserSetting : BaseTable
{
    public const string TableName = "UserSetting";

    [Required]
    public Guid UserID { get; set; }

    [ForeignKey(nameof(UserID))]
    public TableUser? User { get; set; }

    [Required]
    [MaxLength(64)]
    public required string Key { get; set; }

    [Required]
    public DateTime TimeModified { get; set; }
}
