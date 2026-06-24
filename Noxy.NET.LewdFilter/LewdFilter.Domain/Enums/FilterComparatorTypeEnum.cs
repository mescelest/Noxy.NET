using System.ComponentModel;

namespace LewdFilter.Domain.Enums;

public enum FilterComparatorTypeEnum
{
    Equal = 0,
    NotEqual = 1,
    LessThan = 2,
    LessThanOrEqual = 3,
    GreaterThan = 4,
    GreaterThanOrEqual = 5,
    Exact = 6,
}

public static class ComparatorTypeEnumExtensions
{
    extension(FilterComparatorTypeEnum filterComparator)
    {
        public string ToFilterString() => filterComparator switch
        {
            FilterComparatorTypeEnum.Equal => "=",
            FilterComparatorTypeEnum.NotEqual => "!",
            FilterComparatorTypeEnum.LessThan => "<",
            FilterComparatorTypeEnum.LessThanOrEqual => "<=",
            FilterComparatorTypeEnum.GreaterThan => ">",
            FilterComparatorTypeEnum.GreaterThanOrEqual => ">=",
            FilterComparatorTypeEnum.Exact => "==",
            _ => throw new InvalidEnumArgumentException(nameof(filterComparator), (int)filterComparator, typeof(FilterComparatorTypeEnum))
        };

        public string ToTextString() => filterComparator switch
        {
            FilterComparatorTypeEnum.Equal => "=",
            FilterComparatorTypeEnum.NotEqual => "≠",
            FilterComparatorTypeEnum.LessThan => "<",
            FilterComparatorTypeEnum.LessThanOrEqual => "≤",
            FilterComparatorTypeEnum.GreaterThan => ">",
            FilterComparatorTypeEnum.GreaterThanOrEqual => "≥",
            FilterComparatorTypeEnum.Exact => "⩵",
            _ => throw new InvalidEnumArgumentException(nameof(filterComparator), (int)filterComparator, typeof(FilterComparatorTypeEnum))
        };
    }
}
