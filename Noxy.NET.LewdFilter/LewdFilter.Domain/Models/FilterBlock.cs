using LewdFilter.Domain.Abstractions;

namespace LewdFilter.Domain.Models;

public record FilterBlock : FilterEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsShow { get; set; } = true;

    public HashSet<string> ClassList { get; set; } = [];
    public HashSet<string> BaseTypeList { get; set; } = [];
    public IReadOnlyList<FilterRule> RuleList { get; set; } = [];

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

    protected FilterBlock(FilterBlock other) : base(other)
    {
        Name = other.Name;
        IsShow = other.IsShow;
        ClassList = [.. other.ClassList];
        BaseTypeList = [.. other.BaseTypeList];
        RuleList = other.RuleList.Select(r => r with { }).ToList();
    }

    public List<FilterRule> MoveRule(Guid ruleId, int destinationIndex)
    {
        List<FilterRule> list = RuleList.ToList();
        int oldIndex = list.FindIndex(r => r.ID == ruleId);
        if (oldIndex == -1 || destinationIndex < 0 || destinationIndex >= list.Count || oldIndex == destinationIndex) return list;

        FilterRule item = list[oldIndex];
        list.RemoveAt(oldIndex);
        list.Insert(destinationIndex, item);
        return list;
    }

    public List<FilterRule> RemoveRule(Guid ruleId)
    {
        return RuleList.Where(r => r.ID != ruleId).ToList();
    }
}
