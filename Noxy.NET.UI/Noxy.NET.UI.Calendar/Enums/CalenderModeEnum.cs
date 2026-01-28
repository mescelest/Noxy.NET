using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum CalenderModeEnum
{
    Year = 1,
    Month = 2,
    Week = 3,
    Day = 4
}

public static class CalenderModeEnumExtensions
{
    public static string ToText(this CalenderModeEnum value)
    {
        return value switch
        {
            CalenderModeEnum.Day => "Day",
            CalenderModeEnum.Week => "Week",
            CalenderModeEnum.Month => "Month",
            CalenderModeEnum.Year => "Year",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(CalenderModeEnum))
        };
    }
}
