using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum IconSizeEnum
{
    ExtraExtraSmall,
    ExtraSmall,
    Small,
    Medium,
    Large,
    ExtraLarge,
    ExtraExtraLarge
}

public static class IconSizeEnumExtensions
{
    public static string ToText(this IconSizeEnum value)
    {
        return value switch
        {
            IconSizeEnum.ExtraExtraSmall => "xxs",
            IconSizeEnum.ExtraSmall => "xs",
            IconSizeEnum.Small => "sm",
            IconSizeEnum.Medium => "md",
            IconSizeEnum.Large => "lg",
            IconSizeEnum.ExtraLarge => "xl",
            IconSizeEnum.ExtraExtraLarge => "xxl",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(IconSizeEnum))
        };
    }
}
