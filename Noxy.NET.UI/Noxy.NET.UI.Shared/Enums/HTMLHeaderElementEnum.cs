using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum HTMLHeaderElementEnum
{
    H1,
    H2,
    H3,
    H4,
    H5,
    H6
}

public static class HTMLHeaderElementEnumExtensions
{
    public static string ToText(this HTMLHeaderElementEnum value)
    {
        return value switch
        {
            HTMLHeaderElementEnum.H1 => "h1",
            HTMLHeaderElementEnum.H2 => "h2",
            HTMLHeaderElementEnum.H3 => "h3",
            HTMLHeaderElementEnum.H4 => "h4",
            HTMLHeaderElementEnum.H5 => "h5",
            HTMLHeaderElementEnum.H6 => "h6",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(HTMLTextAlignmentEnum))
        };
    }

    public static HTMLHeaderElementEnum GetNext(this HTMLHeaderElementEnum value)
    {
        return value switch
        {
            HTMLHeaderElementEnum.H1 => HTMLHeaderElementEnum.H2,
            HTMLHeaderElementEnum.H2 => HTMLHeaderElementEnum.H3,
            HTMLHeaderElementEnum.H3 => HTMLHeaderElementEnum.H4,
            HTMLHeaderElementEnum.H4 => HTMLHeaderElementEnum.H5,
            HTMLHeaderElementEnum.H5 => HTMLHeaderElementEnum.H6,
            HTMLHeaderElementEnum.H6 => HTMLHeaderElementEnum.H6,
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(HTMLTextAlignmentEnum))
        };
    }
}
