using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Declarations;

public abstract record SettingDeclarationEnum(string Key, IReadOnlyList<string> MemberNameList, string DefaultName) : SettingDeclaration(Key, SettingValueTypeEnum.Enum)
{
    public override object DefaultValue => DefaultName;

    // Exactly a member name: no other case, no number.
    public override object ParseValue(JsonElement value)
    {
        string? name = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        if (name is null || !MemberNameList.Contains(name, StringComparer.Ordinal)) throw Invalid();

        return name;
    }

    public override object ReadValue(EntityUserSetting? saved)
    {
        return saved is EntityUserSettingEnum member && MemberNameList.Contains(member.Value, StringComparer.Ordinal) ? member.Value : DefaultName;
    }

    public override EntityUserSetting CreateEntity(Guid userID, object value)
    {
        return new EntityUserSettingEnum { UserID = userID, Key = Key, Value = (string)value };
    }

    public override bool SetValue(EntityUserSetting saved, object value)
    {
        if (saved is not EntityUserSettingEnum member) throw WrongRowType(saved);

        string next = (string)value;
        if (member.Value == next) return false;

        member.Value = next;
        return true;
    }
}
