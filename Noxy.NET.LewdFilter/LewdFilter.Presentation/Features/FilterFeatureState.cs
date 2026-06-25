using System.Collections.Immutable;
using Fluxor;
using LewdFilter.Domain.Enums;
using LewdFilter.Domain.Models;

namespace LewdFilter.Presentation.Features;

[FeatureState]
public record FilterFeatureState
{
    public Filter Filter { get; init; } = new()
    {
        Name = "MyFilter",
        GroupList = [FilterGroup.Default],
        CustomColorCollection = new()
        {
            [FilterColorTypeEnum.Border] = [FilterColor.DefaultBorder],
            [FilterColorTypeEnum.Text] = [FilterColor.DefaultText],
            [FilterColorTypeEnum.Background] = [FilterColor.DefaultBackground],
        }
    };

    public bool IsLoading { get; init; }

    public static Func<FilterFeatureState, FilterGroup> SelectGroup(Guid groupId)
    {
        return state => state.Filter.GroupList.FirstOrDefault(group => group.ID == groupId) ?? FilterGroup.Default;
    }

    public static Func<FilterFeatureState, string> SelectGroupName(Guid groupId)
    {
        return state => SelectGroup(groupId)(state).Name;
    }

    public static Func<FilterFeatureState, ImmutableArray<FilterGroup>> SelectGroupList()
    {
        return state => [.. state.Filter.GroupList];
    }

    public static Func<FilterFeatureState, ImmutableArray<Guid>> SelectGroupIDList()
    {
        return state => [.. SelectGroupList()(state).Select(group => group.ID)];
    }

    public static Func<FilterFeatureState, FilterBlock> SelectBlock(Guid groupId, Guid blockId)
    {
        return state => SelectGroup(groupId)(state).BlockList.FirstOrDefault(block => block.ID == blockId) ?? FilterBlock.Default;
    }

    public static Func<FilterFeatureState, ImmutableArray<Guid>> SelectBlockIDList(Guid groupId)
    {
        return state => [.. SelectGroup(groupId)(state).BlockList.Select(block => block.ID)];
    }

    public static Func<FilterFeatureState, int> SelectBlockBaseTypeCount(Guid groupId, Guid blockId)
    {
        return state => SelectBlock(groupId, blockId)(state).BaseTypeList.Count;
    }

    public static Func<FilterFeatureState, int> SelectBlockRuleCount(Guid groupId, Guid blockId)
    {
        return state => SelectBlock(groupId, blockId)(state).RuleList.Count;
    }

    public static Func<FilterFeatureState, ImmutableArray<FilterRule>> SelectRuleList(Guid groupId, Guid blockId)
    {
        return state => [.. SelectBlock(groupId, blockId)(state).RuleList];
    }

    public static Func<FilterFeatureState, ImmutableArray<Guid>> SelectRuleIDList(Guid groupId, Guid blockId)
    {
        return state => { return [.. SelectRuleList(groupId, blockId)(state).Select(rule => rule.ID)]; };
    }

    public static Func<FilterFeatureState, FilterRule> SelectRule(Guid groupId, Guid blockId, Guid ruleId)
    {
        return state => { return SelectBlock(groupId, blockId)(state).RuleList.FirstOrDefault(rule => rule.ID == ruleId) ?? FilterRule.Default; };
    }

    public static Func<FilterFeatureState, ImmutableDictionary<FilterColorTypeEnum, IReadOnlyList<FilterColor>>> SelectCustomColorList()
    {
        return state => state.Filter.CustomColorCollection.ToImmutableDictionary();
    }

    public static Func<FilterFeatureState, ImmutableArray<FilterColor>> SelectColorListByTab(FilterColorTypeEnum tab)
    {
        return state => state.Filter.CustomColorCollection.TryGetValue(tab, out IReadOnlyList<FilterColor>? list) ? [.. list] : [];
    }

    public static Func<FilterFeatureState, string> SelectPreviewStyle(Guid groupId, Guid blockId)
    {
        return state =>
        {
            FilterBlock block = SelectBlock(groupId, blockId)(state);
            FilterColor textColor = block.RuleList.OfType<FilterRule<FilterColor?>>().FirstOrDefault(rule => rule.Keyword == FilterKeywordEnum.SetTextColor)?.Value ?? FilterColor.DefaultText;
            FilterColor bgColor = block.RuleList.OfType<FilterRule<FilterColor?>>().FirstOrDefault(rule => rule.Keyword == FilterKeywordEnum.SetBackgroundColor)?.Value ?? FilterColor.DefaultBackground;
            FilterColor borderColor = block.RuleList.OfType<FilterRule<FilterColor?>>().FirstOrDefault(rule => rule.Keyword == FilterKeywordEnum.SetBorderColor)?.Value ?? FilterColor.DefaultBorder;
            double rawFontSize = block.RuleList.OfType<FilterRule<FilterFontSize>>().FirstOrDefault(rule => rule.Keyword == FilterKeywordEnum.SetFontSize)?.Value?.Value ?? 32.0;

            return $"color: {textColor.ToHex()}; background-color: {bgColor.ToHex()}; border: 1px solid {borderColor.ToHex()}; font-size: {rawFontSize / 32.0:0.00}rem;";
        };
    }
}
