using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum HTMLSpellcheckEnum
{
    Default,
    True,
    False
}

public static class HTMLSpellcheckEnumExtensions
{
    public static string ToText(this HTMLSpellcheckEnum value)
    {
        return value switch
        {
            HTMLSpellcheckEnum.Default => "default",
            HTMLSpellcheckEnum.True => "true",
            HTMLSpellcheckEnum.False => "false",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(HTMLSpellcheckEnum))
        };
    }
}
