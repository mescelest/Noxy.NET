using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum HTMLPreserveAspectRatioAlignEnum
{
    None,
    XMinYMin,
    XMidYMin,
    XMaxYMin,
    XMinYMid,
    XMidYMid,
    XMaxYMid,
    XMinYMax,
    XMidYMax,
    XMaxYMax
}

public static class HTMLPreserveAspectRatioAlignEnumExtensions
{
    public static string ToText(this HTMLPreserveAspectRatioAlignEnum value)
    {
        return value switch
        {
            HTMLPreserveAspectRatioAlignEnum.None => "none",
            HTMLPreserveAspectRatioAlignEnum.XMinYMin => "xMinYMin",
            HTMLPreserveAspectRatioAlignEnum.XMidYMin => "xMidYMin",
            HTMLPreserveAspectRatioAlignEnum.XMaxYMin => "xMaxYMin",
            HTMLPreserveAspectRatioAlignEnum.XMinYMid => "xMinYMid",
            HTMLPreserveAspectRatioAlignEnum.XMidYMid => "xMidYMid",
            HTMLPreserveAspectRatioAlignEnum.XMaxYMid => "xMaxYMid",
            HTMLPreserveAspectRatioAlignEnum.XMinYMax => "xMinYMax",
            HTMLPreserveAspectRatioAlignEnum.XMidYMax => "xMidYMax",
            HTMLPreserveAspectRatioAlignEnum.XMaxYMax => "xMaxYMax",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(HTMLPreserveAspectRatioAlignEnum))
        };
    }
}
