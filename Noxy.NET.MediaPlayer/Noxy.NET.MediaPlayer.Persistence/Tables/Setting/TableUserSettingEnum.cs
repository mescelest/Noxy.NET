using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Setting;

[Table("UserSettingEnum")]
public class TableUserSettingEnum : TableUserSetting
{
    [Required]
    [MaxLength(64)]
    public required string Value { get; set; }
}
