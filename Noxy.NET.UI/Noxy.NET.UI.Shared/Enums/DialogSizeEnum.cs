using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum DialogSizeEnum
{
    Default,
    Medium,
    Wide,
    Full
}

public static class DialogSizeEnumExtensions
{
    public static string ToText(this DialogSizeEnum value)
    {
        return value switch
        {
            DialogSizeEnum.Default => "default",
            DialogSizeEnum.Medium => "medium",
            DialogSizeEnum.Wide => "wide",
            DialogSizeEnum.Full => "full",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(DialogSizeEnum))
        };
    }
}
