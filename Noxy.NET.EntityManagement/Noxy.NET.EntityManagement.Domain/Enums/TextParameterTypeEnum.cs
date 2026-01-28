using System.ComponentModel;

namespace Noxy.NET.EntityManagement.Domain.Enums;

public enum TextParameterTypeEnum
{
    Line,
    Text,
    RichText
}

public static class TextParameterTypeEnumExtensions
{
    public static string ToText(this TextParameterTypeEnum value)
    {
        return value switch
        {
            TextParameterTypeEnum.Line => "Line",
            TextParameterTypeEnum.Text => "Text",
            TextParameterTypeEnum.RichText => "Rich text",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(TextParameterTypeEnum))
        };
    }
}
