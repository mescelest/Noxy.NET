using LewdFilter.Domain.Abstractions;
using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public class FilterComparatorInt : FilterEntity
{
    public int? Left { get; set; }
    public int? Right { get; set; }
    public ComparatorTypeEnum ComparatorType { get; set; }

    public override string ToString()
    {
        string op = ComparatorType.ToFilterSymbol();
        return (Left, Right) switch
        {
            ({ } l, { } r) => $"{l} {op} {r}",
            ({ } l, null) => $"{l} {op}",
            (null, { } r) => $"{op} {r}",
            (null, null) => string.Empty
        };
    }
}
