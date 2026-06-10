namespace LewdFilter.Domain.Enums;

public enum ComparatorTypeEnum
{
    Equal = 0,
    NotEqual = 1,
    LessThan = 2,
    LessThanOrEqual = 3,
    GreaterThan = 4,
    GreaterThanOrEqual = 5,
    Between = 6,
}

public static class ComparatorTypeEnumExtensions
{
    public static string ToFilterSymbol(this ComparatorTypeEnum comparator) => comparator switch
    {
        ComparatorTypeEnum.Equal => "==",
        ComparatorTypeEnum.NotEqual => "!=",
        ComparatorTypeEnum.LessThan => "<",
        ComparatorTypeEnum.LessThanOrEqual => "<=",
        ComparatorTypeEnum.GreaterThan => ">",
        ComparatorTypeEnum.GreaterThanOrEqual => ">=",
        ComparatorTypeEnum.Between => "BETWEEN",
        _ => string.Empty
    };

    public static string ToTextSymbol(this ComparatorTypeEnum comparator) => comparator switch
    {
        ComparatorTypeEnum.Equal => "⩵",
        ComparatorTypeEnum.NotEqual => "≠",
        ComparatorTypeEnum.LessThan => "<",
        ComparatorTypeEnum.LessThanOrEqual => "≤",
        ComparatorTypeEnum.GreaterThan => ">",
        ComparatorTypeEnum.GreaterThanOrEqual => "≥",
        ComparatorTypeEnum.Between => "≤ x ≤",
        _ => string.Empty
    };
}
