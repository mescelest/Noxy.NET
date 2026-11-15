using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Declarations;

public record SettingDeclarationInteger(string Key, int Default, int Minimum, int Maximum) : SettingDeclaration(Key, SettingValueType.Integer)
{
    public override object DefaultValue => Default;

    public override object ParseValue(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int number) || !IsInRange(number)) throw Invalid();

        return number;
    }

    public override object ReadValue(EntityUserSetting? saved)
    {
        return saved is EntityUserSettingInteger integer && IsInRange(integer.Value) ? integer.Value : Default;
    }

    public override EntityUserSetting CreateEntity(Guid userID, object value)
    {
        return new EntityUserSettingInteger { UserID = userID, Key = Key, Value = (int)value };
    }

    public override bool SetValue(EntityUserSetting saved, object value)
    {
        if (saved is not EntityUserSettingInteger integer) throw WrongRowType(saved);

        int next = (int)value;
        if (integer.Value == next) return false;

        integer.Value = next;
        return true;
    }

    private bool IsInRange(int number)
    {
        return number >= Minimum && number <= Maximum;
    }
}
