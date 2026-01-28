using System.ComponentModel;

namespace Noxy.NET.EntityManagement.Domain.Enums;

public enum DateTimeTypeEnum
{
    Date,
    Time,
    DateAndTime
}

public static class DateTimeTypeEnumExtensions
{
    public static string ToText(this DateTimeTypeEnum value)
    {
        return value switch
        {
            DateTimeTypeEnum.Date => "Date",
            DateTimeTypeEnum.Time => "Time",
            DateTimeTypeEnum.DateAndTime => "Date and time",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(DateTimeTypeEnum))
        };
    }
}
