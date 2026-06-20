namespace LewdFilter.Domain.Enums;

public enum FilterRarityTypeEnum
{
    Normal,
    Magic,
    Rare,
    Unique
}

public static class RarityTypeEnumExtensions
{
    public static string ToFilterString(this FilterRarityTypeEnum filterRarity) => filterRarity switch
    {
        FilterRarityTypeEnum.Normal => "Normal",
        FilterRarityTypeEnum.Magic => "Magic",
        FilterRarityTypeEnum.Rare => "Rare",
        FilterRarityTypeEnum.Unique => "Unique",
        _ => filterRarity.ToString()
    };
}
