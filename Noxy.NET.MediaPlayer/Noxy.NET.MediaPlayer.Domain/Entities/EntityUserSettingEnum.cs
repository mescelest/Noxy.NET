namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityUserSettingEnum : EntityUserSetting
{
    // The name of the enum member, so reordering the enum later cannot change what was saved.
    public required string Value { get; set; }
}
