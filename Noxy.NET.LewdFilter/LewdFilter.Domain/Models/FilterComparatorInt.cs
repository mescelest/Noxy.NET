using LewdFilter.Domain.Abstractions;
using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public record FilterComparatorInt : FilterEntity
{
    public int Value { get; set; }
    public FilterComparatorTypeEnum FilterComparatorType { get; set; }

    public override string ToString()
    {
        return $"{FilterComparatorType.ToFilterSymbol()} {Value}";
    }
}
