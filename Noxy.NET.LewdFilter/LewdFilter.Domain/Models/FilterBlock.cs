using LewdFilter.Domain.Abstractions;

namespace LewdFilter.Domain.Models;

public record FilterBlock : FilterEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsShow { get; set; } = true;

    public HashSet<string> ClassList { get; set; } = [];
    public HashSet<string> BaseTypeList { get; set; } = [];

    public FilterComparatorRarity? Rarity { get; set; }
    public FilterComparatorInt? ItemLevel { get; set; }
    public FilterComparatorInt? AreaLevel { get; set; }
    public FilterComparatorInt? Sockets { get; set; }
    public FilterComparatorInt? Quality { get; set; }
    public FilterComparatorInt? StackSize { get; set; }
    public FilterComparatorInt? UnidentifiedItemTier { get; set; }
    public FilterComparatorInt? WaystoneTier { get; set; }
    public FilterComparatorInt? GemLevel { get; set; }

    public FilterComparatorInt? BaseArmour { get; set; }
    public FilterComparatorInt? BaseEvasion { get; set; }
    public FilterComparatorInt? BaseEnergyShield { get; set; }

    public bool? Corrupted { get; set; }
    public FilterComparatorInt? CorruptedMods { get; set; }
    public bool? AlwaysShow { get; set; }

    public FilterColor? TextColor { get; set; }
    public FilterColor? BorderColor { get; set; }
    public FilterColor? BackgroundColor { get; set; }
    public int? FontSize { get; set; }
    public FilterBeamEffect? PlayEffect { get; set; }
    public FilterMinimapIcon? MinimapIcon { get; set; }

    public static FilterBlock Default => new() { Name = "New block" };

    public FilterBlock()
    {
    }

    protected FilterBlock(FilterBlock other) : base(other)
    {
        Name = other.Name;
        IsShow = other.IsShow;
        ClassList = [.. other.ClassList];
        BaseTypeList = [.. other.BaseTypeList];
        Rarity = other.Rarity;
        ItemLevel = other.ItemLevel;
        AreaLevel = other.AreaLevel;
        Sockets = other.Sockets;
        Quality = other.Quality;
        StackSize = other.StackSize;
        UnidentifiedItemTier = other.UnidentifiedItemTier;
        WaystoneTier = other.WaystoneTier;
        GemLevel = other.GemLevel;
        BaseArmour = other.BaseArmour;
        BaseEvasion = other.BaseEvasion;
        BaseEnergyShield = other.BaseEnergyShield;
        Corrupted = other.Corrupted;
        CorruptedMods = other.CorruptedMods;
        AlwaysShow = other.AlwaysShow;
        TextColor = other.TextColor;
        BorderColor = other.BorderColor;
        BackgroundColor = other.BackgroundColor;
        FontSize = other.FontSize;
        PlayEffect = other.PlayEffect;
        MinimapIcon = other.MinimapIcon;
    }
}
