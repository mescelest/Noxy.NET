using LewdFilter.Domain.Abstractions;
using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public record FilterComparatorRarity : FilterEntity
{
    public FilterRarityTypeEnum Value { get; set; }
    public FilterComparatorTypeEnum FilterComparatorType { get; set; }

    public static FilterComparatorRarity Default => new() { Value = FilterRarityTypeEnum.Normal };

    public override string ToString()
    {
        return $"{FilterComparatorType.ToFilterString()} {Value.ToFilterString()}";
    }
}
