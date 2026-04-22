using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum HTMLHeaderLevelEnum
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
    public static string ToText(this HTMLHeaderLevelEnum value)
    {
        return value switch
        {
            HTMLHeaderLevelEnum.H1 => "h1",
            HTMLHeaderLevelEnum.H2 => "h2",
            HTMLHeaderLevelEnum.H3 => "h3",
            HTMLHeaderLevelEnum.H4 => "h4",
            HTMLHeaderLevelEnum.H5 => "h5",
            HTMLHeaderLevelEnum.H6 => "h6",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(HTMLTextAlignmentEnum))
        };
    }

    public static HTMLHeaderLevelEnum GetNext(this HTMLHeaderLevelEnum value)
    {
        return value switch
        {
            HTMLHeaderLevelEnum.H1 => HTMLHeaderLevelEnum.H2,
            HTMLHeaderLevelEnum.H2 => HTMLHeaderLevelEnum.H3,
            HTMLHeaderLevelEnum.H3 => HTMLHeaderLevelEnum.H4,
            HTMLHeaderLevelEnum.H4 => HTMLHeaderLevelEnum.H5,
            HTMLHeaderLevelEnum.H5 or HTMLHeaderLevelEnum.H6 => HTMLHeaderLevelEnum.H6,
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(HTMLTextAlignmentEnum))
        };
    }
}
