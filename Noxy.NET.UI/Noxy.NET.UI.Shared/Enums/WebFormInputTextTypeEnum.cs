using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum WebFormInputTextTypeEnum
{
    Search,
    Tel,
    Text
}

public static class WebFormInputTextTypeEnumExtensions
{
    public static string ToText(this WebFormInputTextTypeEnum value)
    {
        return value switch
        {
            WebFormInputTextTypeEnum.Search => "search",
            WebFormInputTextTypeEnum.Tel => "tel",
            WebFormInputTextTypeEnum.Text => "text",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(WebFormInputTextTypeEnum))
        };
    }
}
