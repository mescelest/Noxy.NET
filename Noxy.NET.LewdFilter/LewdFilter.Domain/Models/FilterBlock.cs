namespace LewdFilter.Domain.Models;

public class FilterBlock : FilterEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsShow { get; set; } = true;

    public HashSet<string> BaseTypeList { get; set; } = [];

    public FilterComparatorRarity? Rarity { get; set; }
    public FilterComparatorInt? StackSize { get; set; }
    public FilterComparatorInt? Quality { get; set; }
    public FilterComparatorInt? UnidentifiedItemTier { get; set; }

    public FilterColor? TextColor { get; set; }
    public FilterColor? BorderColor { get; set; }
    public FilterColor? BackgroundColor { get; set; }
    public int? FontSize { get; set; }

    public static FilterBlock Default => new() { Name = "New block" };

    public FilterBlock Clone()
    {
        return new()
        {
            ID = ID,
            Name = Name,
            IsShow = IsShow,
            BaseTypeList = BaseTypeList,
            Rarity = Rarity,
            StackSize = StackSize,
            Quality = Quality,
            UnidentifiedItemTier = UnidentifiedItemTier,
            TextColor = TextColor,
            BorderColor = BorderColor,
            BackgroundColor = BackgroundColor,
            FontSize = FontSize
        };
    }
}
