using System.ComponentModel;
using System.Text.Json.Serialization;
using LewdFilter.Domain.Abstractions;
using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(FilterRule), nameof(FilterRule))]
[JsonDerivedType(typeof(FilterRule<bool>), nameof(Boolean))]
[JsonDerivedType(typeof(FilterRule<int>), nameof(Int32))]
[JsonDerivedType(typeof(FilterRule<string>), nameof(String))]
[JsonDerivedType(typeof(FilterRule<FilterColor?>), nameof(FilterColor))]
[JsonDerivedType(typeof(FilterRule<FilterFontSize>), nameof(FilterFontSize))]
[JsonDerivedType(typeof(FilterRule<FilterBeamEffect?>), nameof(FilterBeamEffect))]
[JsonDerivedType(typeof(FilterRule<FilterMinimapIcon?>), nameof(FilterMinimapIcon))]
[JsonDerivedType(typeof(FilterRule<FilterComparatorRarity?>), nameof(FilterComparatorRarity))]
[JsonDerivedType(typeof(FilterRule<FilterComparatorInt?>), nameof(FilterComparatorInt))]
public record FilterRule : FilterEntity
{
    public FilterKeywordEnum Keyword { get; set; }

    public static FilterRule Default => new() { Keyword = FilterKeywordEnum.SetTextColor };

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

            FilterKeywordEnum.SetTextColor =>
                new FilterRule<FilterColor?> { Keyword = keyword, Value = FilterColor.DefaultText },

            FilterKeywordEnum.SetBorderColor =>
                new FilterRule<FilterColor?> { Keyword = keyword, Value = FilterColor.DefaultBorder },

            FilterKeywordEnum.SetBackgroundColor =>
                new FilterRule<FilterColor?> { Keyword = keyword, Value = FilterColor.DefaultBackground },

            FilterKeywordEnum.SetFontSize =>
                new FilterRule<FilterFontSize> { Keyword = keyword, Value = new(32) },

            FilterKeywordEnum.PlayEffect =>
                new FilterRule<FilterBeamEffect?> { Keyword = keyword, Value = new(FilterColorNameEnum.Red) },

            FilterKeywordEnum.MinimapIcon =>
                new FilterRule<FilterMinimapIcon?> { Keyword = keyword, Value = new(FilterColorNameEnum.Red, FilterIconSizeEnum.Small, FilterIconShapeEnum.Circle) },

            FilterKeywordEnum.PlayAlertSound
                or FilterKeywordEnum.PlayAlertSoundPositional =>
                new FilterRule<int> { Keyword = keyword, Value = 1 },

            FilterKeywordEnum.CustomAlertSound
                or FilterKeywordEnum.CustomAlertSoundOptional =>
                new FilterRule<string> { Keyword = keyword, Value = string.Empty },

            FilterKeywordEnum.Rarity =>
                new FilterRule<FilterComparatorRarity?> { Keyword = keyword, Value = new() },

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
                => new FilterRule<FilterComparatorInt?> { Keyword = keyword, Value = new() },

            _ => throw new InvalidEnumArgumentException(nameof(keyword), (int)keyword, typeof(FilterKeywordEnum))
        };
    }
}

public record FilterRule<T> : FilterRule
{
    public required T Value { get; set; }

    public override string ValueAsString() => Value switch
    {
        null => string.Empty,
        bool x => x ? bool.TrueString : bool.FalseString,
        int x => x.ToString(),
        string x => x,
        FilterColor x => x.ToString(),
        FilterBeamEffect x => x.ToString(),
        FilterComparatorInt x => x.ToString(),
        FilterComparatorRarity r => r.ToString(),
        FilterMinimapIcon x => x.ToString(),
        FilterFontSize x => x.ToString(),
        _ => throw new InvalidOperationException($"Type {typeof(T).Name} is missing a serialization case.")
    };
}
