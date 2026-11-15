using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Setting;

[Table("UserSettingBoolean")]
public class TableUserSettingBoolean : TableUserSetting
{
    [Required]
    public bool Value { get; set; }
}
