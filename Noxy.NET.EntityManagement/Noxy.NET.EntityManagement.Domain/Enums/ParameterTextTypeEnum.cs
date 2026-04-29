using System.ComponentModel;

namespace Noxy.NET.EntityManagement.Domain.Enums;

public enum ParameterTextTypeEnum
{
    Line,
    Text,
    RichText
}

public static class ParameterTextTypeEnumExtensions
{
    public static string ToText(this ParameterTextTypeEnum value)
    {
        return value switch
        {
            ParameterTextTypeEnum.Line => "Line",
            ParameterTextTypeEnum.Text => "Text",
            ParameterTextTypeEnum.RichText => "Rich text",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ParameterTextTypeEnum))
        };
    }
}
