using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum HTMLTextAlignmentEnum
{
    Left,
    Center,
    Right,
    Justify
}

public static class HTMLTextAlignmentEnumExtensions
{
    public static string ToText(this HTMLTextAlignmentEnum value)
    {
        return value switch
        {
            HTMLTextAlignmentEnum.Left => "left",
            HTMLTextAlignmentEnum.Right => "right",
            HTMLTextAlignmentEnum.Center => "center",
            HTMLTextAlignmentEnum.Justify => "justify",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(HTMLTextAlignmentEnum))
        };
    }
}
