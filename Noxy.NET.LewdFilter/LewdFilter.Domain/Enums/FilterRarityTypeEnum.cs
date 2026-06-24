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
    extension(FilterRarityTypeEnum filterRarity)
    {
        public string ToFilterString() => filterRarity switch
        {
            FilterRarityTypeEnum.Normal => "Normal",
            FilterRarityTypeEnum.Magic => "Magic",
            FilterRarityTypeEnum.Rare => "Rare",
            FilterRarityTypeEnum.Unique => "Unique",
            _ => filterRarity.ToString()
        };

        public string ToTextString() => filterRarity switch
        {
            FilterRarityTypeEnum.Normal => "Normal",
            FilterRarityTypeEnum.Magic => "Magic",
            FilterRarityTypeEnum.Rare => "Rare",
            FilterRarityTypeEnum.Unique => "Unique",
            _ => filterRarity.ToString()
        };
    }
}
