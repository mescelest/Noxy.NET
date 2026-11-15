using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Declarations;

public record SettingDeclarationBoolean(string Key, bool Default) : SettingDeclaration(Key, SettingValueType.Boolean)
{
    public override object DefaultValue => Default;

    public override object ParseValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw Invalid()
        };
    }

    public override object ReadValue(EntityUserSetting? saved)
    {
        return saved is EntityUserSettingBoolean boolean ? boolean.Value : Default;
    }

    public override EntityUserSetting CreateEntity(Guid userID, object value)
    {
        return new EntityUserSettingBoolean { UserID = userID, Key = Key, Value = (bool)value };
    }

    public override bool SetValue(EntityUserSetting saved, object value)
    {
        if (saved is not EntityUserSettingBoolean boolean) throw WrongRowType(saved);

        bool next = (bool)value;
        if (boolean.Value == next) return false;

        boolean.Value = next;
        return true;
    }
}
