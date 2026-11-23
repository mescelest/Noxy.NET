using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum ButtonAppearanceEnum
{
    Filled,
    Outline,
    Ghost
}

public static class ButtonAppearanceEnumExtensions
{
    public static string ToText(this ButtonAppearanceEnum value)
    {
        return value switch
        {
            ButtonAppearanceEnum.Filled => "filled",
            ButtonAppearanceEnum.Outline => "outline",
            ButtonAppearanceEnum.Ghost => "ghost",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ButtonAppearanceEnum))
        };
    }
}
