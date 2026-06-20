namespace LewdFilter.Domain.Enums;

public enum FilterIconSizeEnum
{
    Small = 2,
    Medium = 1,
    Large = 0
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
            _ => "1"
        };

        public string ToTextString() => size switch
        {
            FilterIconSizeEnum.Large => "Large",
            FilterIconSizeEnum.Medium => "Medium",
            FilterIconSizeEnum.Small => "Small",
            _ => string.Empty
        };
    }
}
