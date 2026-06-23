using System.ComponentModel;
using LewdFilter.Domain.Abstractions;
using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public record FilterRule : FilterEntity
{
    public FilterKeywordEnum Keyword { get; set; }

    public static FilterRule Default => new();

    public virtual string ValueAsString() => string.Empty;

    public string ToFilterLine() => $"{Keyword} {ValueAsString()}".Trim();

    public static FilterRule FromKeyword(FilterKeywordEnum keyword)
    {
        return keyword switch
        {
            FilterKeywordEnum.DisableDropSound or FilterKeywordEnum.EnableDropSound =>
                new() { Keyword = keyword },

            FilterKeywordEnum.AlwaysShow
                or FilterKeywordEnum.Mirrored
                or FilterKeywordEnum.Corrupted
                or FilterKeywordEnum.FracturedItem
                or FilterKeywordEnum.Identified
                or FilterKeywordEnum.IsVaalUnique
                or FilterKeywordEnum.TwiceCorrupted =>
                new FilterRule<bool> { Keyword = keyword, Value = true },

            FilterKeywordEnum.Rarity =>
                new FilterRule<FilterComparatorRarity> { Keyword = keyword, Value = new() },

            FilterKeywordEnum.SetTextColor =>
                new FilterRule<FilterColor> { Keyword = keyword, Value = FilterColor.DefaultText },

            FilterKeywordEnum.SetBorderColor =>
                new FilterRule<FilterColor> { Keyword = keyword, Value = FilterColor.DefaultBorder },

            FilterKeywordEnum.SetBackgroundColor =>
                new FilterRule<FilterColor> { Keyword = keyword, Value = FilterColor.DefaultBackground },

            FilterKeywordEnum.SetFontSize =>
                new FilterRule<int> { Keyword = keyword, Value = 32 },

            FilterKeywordEnum.PlayEffect =>
                new FilterRule<FilterBeamEffect> { Keyword = keyword, Value = new(FilterColorNameEnum.Red) },

            FilterKeywordEnum.MinimapIcon =>
                new FilterRule<FilterMinimapIcon> { Keyword = keyword, Value = new(FilterColorNameEnum.Red, FilterIconSizeEnum.Small, FilterIconShapeEnum.Circle) },

            FilterKeywordEnum.PlayAlertSound
                or FilterKeywordEnum.PlayAlertSoundPositional =>
                new FilterRule<int> { Keyword = keyword, Value = 1 },

            FilterKeywordEnum.CustomAlertSound
                or FilterKeywordEnum.CustomAlertSoundOptional =>
                new FilterRule<string> { Keyword = keyword, Value = string.Empty },

            FilterKeywordEnum.AreaLevel
                or FilterKeywordEnum.DropLevel
                or FilterKeywordEnum.ItemLevel
                or FilterKeywordEnum.WaystoneTier
                or FilterKeywordEnum.GemLevel
                or FilterKeywordEnum.BaseArmour
                or FilterKeywordEnum.BaseEnergyShield
                or FilterKeywordEnum.BaseEvasion
                or FilterKeywordEnum.BaseWard
                or FilterKeywordEnum.Height
                or FilterKeywordEnum.Width
                or FilterKeywordEnum.Quality
                or FilterKeywordEnum.Sockets
                or FilterKeywordEnum.StackSize
                => new FilterRule<FilterComparatorInt> { Keyword = keyword, Value = new() },

            _ => throw new InvalidEnumArgumentException(nameof(keyword), (int)keyword, typeof(FilterKeywordEnum))
        };
    }
}

public record FilterRule<T> : FilterRule
{
    public T Value { get; set; } = default!;

    public override string ValueAsString() => Value switch
    {
        null => string.Empty,
        bool b => b ? "True" : "False",
        FilterComparatorInt x => x.ToString(),
        FilterComparatorRarity r => r.ToString(),
        FilterMinimapIcon x => x.ToString(),
        _ => throw new InvalidOperationException()
    };
}
