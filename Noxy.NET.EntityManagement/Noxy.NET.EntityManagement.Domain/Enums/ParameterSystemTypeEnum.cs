using System.ComponentModel;

namespace Noxy.NET.EntityManagement.Domain.Enums;

public enum ParameterSystemTypeEnum
{
    Boolean,
    String,
    Integer,
    Decimal,
    DateTime,
    Guid
}

public static class ParameterSystemTypeEnumExtensions
{
    public static string ToText(this ParameterSystemTypeEnum value)
    {
        return value switch
        {
            ParameterSystemTypeEnum.Boolean => "Boolean",
            ParameterSystemTypeEnum.String => "String",
            ParameterSystemTypeEnum.Integer => "Integer",
            ParameterSystemTypeEnum.Decimal => "Decimal",
            ParameterSystemTypeEnum.DateTime => "DateTime",
            ParameterSystemTypeEnum.Guid => "Guid",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ParameterSystemTypeEnum))
        };
    }
}
