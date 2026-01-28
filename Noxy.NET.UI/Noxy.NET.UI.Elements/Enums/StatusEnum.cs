using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum StatusEnum
{
    None,
    Error,
    Warning,
    Success,
    Info,
    Highlight,
    Notice
}

public static class WebFormFieldStateEnumExtensions
{
    public static string ToText(this StatusEnum value)
    {
        return value switch
        {
            StatusEnum.None => "",
            StatusEnum.Error => "error",
            StatusEnum.Warning => "warning",
            StatusEnum.Success => "success",
            StatusEnum.Info => "info",
            StatusEnum.Highlight => "highlight",
            StatusEnum.Notice => "notice",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(StatusEnum))
        };
    }
}
