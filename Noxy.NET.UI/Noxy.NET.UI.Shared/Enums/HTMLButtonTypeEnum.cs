using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum HTMLButtonTypeEnum
{
    Button,
    Submit,
    Reset
}

public static class HTMLButtonTypeEnumExtensions
{
    public static string ToText(this HTMLButtonTypeEnum value)
    {
        return value switch
        {
            HTMLButtonTypeEnum.Button => "button",
            HTMLButtonTypeEnum.Submit => "submit",
            HTMLButtonTypeEnum.Reset => "reset",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(HTMLTextAlignmentEnum))
        };
    }
}
