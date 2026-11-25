using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum DialogHeightEnum
{
    Auto,
    Fixed
}

public static class DialogHeightEnumExtensions
{
    public static string ToText(this DialogHeightEnum value)
    {
        return value switch
        {
            DialogHeightEnum.Auto => "auto",
            DialogHeightEnum.Fixed => "fixed",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(DialogHeightEnum))
        };
    }
}
