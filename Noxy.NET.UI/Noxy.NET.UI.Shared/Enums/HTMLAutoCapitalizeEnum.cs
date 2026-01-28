using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum HTMLAutoCapitalizeEnum
{
    On,
    Off,
    None,
    Sentences,
    Words,
    Characters
}

public static class HTMLAutoCapitalizeEnumExtensions
{
    public static string ToText(this HTMLAutoCapitalizeEnum value)
    {
        return value switch
        {
            HTMLAutoCapitalizeEnum.On => "on",
            HTMLAutoCapitalizeEnum.Off => "off",
            HTMLAutoCapitalizeEnum.None => "none",
            HTMLAutoCapitalizeEnum.Sentences => "sentences",
            HTMLAutoCapitalizeEnum.Words => "words",
            HTMLAutoCapitalizeEnum.Characters => "characters",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(HTMLTextAlignmentEnum))
        };
    }
}
