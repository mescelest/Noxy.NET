using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum ButtonListAlignmentEnum
{
    Left,
    Center,
    Right,
}

public static class ButtonListAlignmentEnumExtensions
{
    public static string ToText(this ButtonListAlignmentEnum value)
    {
        return value switch
        {
            ButtonListAlignmentEnum.Left => "left",
            ButtonListAlignmentEnum.Center => "center",
            ButtonListAlignmentEnum.Right => "right",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ButtonListAlignmentEnum))
        };
    }
}
