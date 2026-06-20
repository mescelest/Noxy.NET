using LewdFilter.Domain.Abstractions;
using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public record FilterComparatorRarity : FilterEntity
{
    public FilterRarityTypeEnum? Left { get; set; }
    public FilterRarityTypeEnum? Right { get; set; }
    public FilterComparatorTypeEnum FilterComparatorType { get; set; }

    public override string ToString()
    {
        if (Left is null && Right is null) return string.Empty;

        string op = FilterComparatorType.ToFilterSymbol();
        if (FilterComparatorType != FilterComparatorTypeEnum.Between) return Left is not null ? $"{op} {Left}" : $"{op} {Right}";
        if (Left is not null && Right is null) return $">= {Left}";
        if (Left is null && Right is not null) return $"<= {Right}";

        return $"{op} {Left} {Right}";
    }
}
