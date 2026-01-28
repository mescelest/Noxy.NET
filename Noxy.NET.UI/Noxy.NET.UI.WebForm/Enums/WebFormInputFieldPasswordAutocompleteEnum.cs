using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum WebFormInputFieldPasswordAutocompleteEnum
{
    On,
    Off,
    CurrentPassword,
    NewPassword,
    OneTimeCode
}

public static class WebFormInputFieldPasswordAutocompleteEnumExtensions
{
    public static string ToAttributeValue(this WebFormInputFieldPasswordAutocompleteEnum value)
    {
        return value switch
        {
            WebFormInputFieldPasswordAutocompleteEnum.On => "on",
            WebFormInputFieldPasswordAutocompleteEnum.Off => "off",
            WebFormInputFieldPasswordAutocompleteEnum.NewPassword => "new-password",
            WebFormInputFieldPasswordAutocompleteEnum.CurrentPassword => "current-password",
            WebFormInputFieldPasswordAutocompleteEnum.OneTimeCode => "one-time-code",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(WebFormInputFieldPasswordAutocompleteEnum))
        };
    }
}
