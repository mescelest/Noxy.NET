using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum ButtonSizeEnum
{
    ExtraExtraSmall,
    ExtraSmall,
    Small,
    Medium,
    Large,
    ExtraLarge,
    ExtraExtraLarge
}

public static class ButtonSizeEnumExtensions
{
    public static string ToText(this ButtonSizeEnum value)
    {
        return value switch
        {
            ButtonSizeEnum.ExtraExtraSmall => "xxs",
            ButtonSizeEnum.ExtraSmall => "xs",
            ButtonSizeEnum.Small => "sm",
            ButtonSizeEnum.Medium => "md",
            ButtonSizeEnum.Large => "lg",
            ButtonSizeEnum.ExtraLarge => "xl",
            ButtonSizeEnum.ExtraExtraLarge => "xxl",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ButtonSizeEnum))
        };
    }
}
