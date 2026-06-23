using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum WebFormInputElementPasswordAutocompleteEnum
{
    On,
    Off,
    CurrentPassword,
    NewPassword,
    OneTimeCode
}

public static class WebFormInputElementPasswordAutocompleteEnumExtensions
{
    public static string ToAttributeValue(this WebFormInputElementPasswordAutocompleteEnum value)
    {
        return value switch
        {
            WebFormInputElementPasswordAutocompleteEnum.On => "on",
            WebFormInputElementPasswordAutocompleteEnum.Off => "off",
            WebFormInputElementPasswordAutocompleteEnum.NewPassword => "new-password",
            WebFormInputElementPasswordAutocompleteEnum.CurrentPassword => "current-password",
            WebFormInputElementPasswordAutocompleteEnum.OneTimeCode => "one-time-code",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(WebFormInputElementPasswordAutocompleteEnum))
        };
    }
}
