using LewdFilter.Domain.Abstractions;
using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public record FilterComparatorInt : FilterEntity
{
    public int? Left { get; set; }
    public int? Right { get; set; }
    public FilterComparatorTypeEnum FilterComparatorType { get; set; }

    public override string ToString()
    {
        string op = FilterComparatorType.ToFilterSymbol();
        return (Left, Right) switch
        {
            ({ } l, { } r) => $"{l} {op} {r}",
            ({ } l, null) => $"{l} {op}",
            (null, { } r) => $"{op} {r}",
            (null, null) => string.Empty
        };
    }
}
