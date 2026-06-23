using System.ComponentModel;

namespace LewdFilter.Domain.Enums;

public enum FilterIconSizeEnum
{
    Small = 0,
    Medium = 1,
    Large = 2
}

public static class FilterIconSizeEnumExtensions
{
    extension(FilterIconSizeEnum size)
    {
        public string ToFilterString() => size switch
        {
            FilterIconSizeEnum.Large => "0",
            FilterIconSizeEnum.Medium => "1",
            FilterIconSizeEnum.Small => "2",
            _ => throw new InvalidEnumArgumentException(nameof(size), (int)size, typeof(FilterIconSizeEnum))
        };

        public string ToTextString() => size switch
        {
            FilterIconSizeEnum.Large => "Large",
            FilterIconSizeEnum.Medium => "Medium",
            FilterIconSizeEnum.Small => "Small",
            _ => throw new InvalidEnumArgumentException(nameof(size), (int)size, typeof(FilterIconSizeEnum))
        };
    }
}
