namespace LewdFilter.Domain.Enums;

public enum RarityTypeEnum
{
    Normal,
    Magic,
    Rare,
    Unique
}

public static class RarityTypeEnumExtensions
{
    public static string ToFilterString(this RarityTypeEnum rarity) => rarity switch
    {
        RarityTypeEnum.Normal => "Normal",
        RarityTypeEnum.Magic => "Magic",
        RarityTypeEnum.Rare => "Rare",
        RarityTypeEnum.Unique => "Unique",
        _ => rarity.ToString()
    };
}
