using LewdFilter.Domain.Abstractions;
using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public record FilterComparatorInt : FilterEntity
{
    public int Value { get; set; }
    public FilterComparatorTypeEnum FilterComparatorType { get; set; }

    public static FilterComparatorInt Default => new() { Value = 0 };

    public override string ToString()
    {
        return $"{FilterComparatorType.ToFilterString()} {Value}";
    }
}
