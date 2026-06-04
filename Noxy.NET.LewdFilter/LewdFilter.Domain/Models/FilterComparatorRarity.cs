using LewdFilter.Domain.Abstractions;
using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public class FilterComparatorRarity : FilterEntity
{
    public RarityTypeEnum? Left { get; set; }
    public RarityTypeEnum? Right { get; set; }
    public ComparatorTypeEnum ComparatorType { get; set; }

    public override string ToString()
    {
        if (Left is null && Right is null) return string.Empty;

        string op = ComparatorType.ToFilterSymbol();
        if (ComparatorType != ComparatorTypeEnum.Between) return Left is not null ? $"Rarity {op} {Left}" : $"Rarity {op} {Right}";
        if (Left is not null && Right is null) return $"Rarity >= {Left}";
        if (Left is null && Right is not null) return $"Rarity <= {Right}";

        return $"Rarity {op} {Left} {Right}";
    }
}
