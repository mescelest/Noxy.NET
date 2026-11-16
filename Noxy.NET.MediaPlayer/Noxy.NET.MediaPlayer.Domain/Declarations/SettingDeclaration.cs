using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.MediaPlayer.Domain.Declarations;

public abstract record SettingDeclaration(string Key, SettingValueTypeEnum ValueType)
{
    public abstract object DefaultValue { get; }

    // The value a client sent, as a bool, an int or an enum member name. Throws InvalidSettingValueException when it is not valid for this setting.
    public abstract object ParseValue(JsonElement value);

    // The saved value, or the default when there is no row, the row is of another type, or its value is no longer valid (a number outside the limits, a member that was removed).
    public abstract object ReadValue(EntityUserSetting? saved);

    public abstract EntityUserSetting CreateEntity(Guid userID, object value);

    // True when the saved row changed. Throws InvalidOperationException for a row of another type, which is a bug in the caller.
    public abstract bool SetValue(EntityUserSetting saved, object value);

    protected InvalidSettingValueException Invalid()
    {
        return new(Key);
    }

    protected InvalidOperationException WrongRowType(EntityUserSetting saved)
    {
        return new($"The saved setting {saved.Key} is a {saved.GetType().Name}, not the {ValueType} the declaration of {Key} expects.");
    }
}
