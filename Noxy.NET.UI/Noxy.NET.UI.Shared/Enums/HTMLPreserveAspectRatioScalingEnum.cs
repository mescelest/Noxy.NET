using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum HTMLPreserveAspectRatioScalingEnum
{
    None,
    Meet,
    Slice
}

public static class HTMLPreserveAspectRatioEnumExtensions
{
    public static string ToText(this HTMLPreserveAspectRatioScalingEnum value)
    {
        return value switch
        {
            HTMLPreserveAspectRatioScalingEnum.None => "none",
            HTMLPreserveAspectRatioScalingEnum.Meet => "meet",
            HTMLPreserveAspectRatioScalingEnum.Slice => "slice",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(HTMLPreserveAspectRatioScalingEnum))
        };
    }
}
