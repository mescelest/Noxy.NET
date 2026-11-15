using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Setting;

[Table("UserSettingInteger")]
public class TableUserSettingInteger : TableUserSetting
{
    [Required]
    public int Value { get; set; }
}
