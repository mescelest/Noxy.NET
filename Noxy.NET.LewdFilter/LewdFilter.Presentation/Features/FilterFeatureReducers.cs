using Fluxor;
using LewdFilter.Domain.Enums;
using LewdFilter.Domain.Models;
using Noxy.NET.Extensions;

namespace LewdFilter.Presentation.Features;

public static class FilterFeatureReducers
{
    public record SetColorAction(FilterColorTypeEnum Type, FilterColor Color);
    public record RemoveColorAction(FilterColorTypeEnum Type, Guid ColorID);

    public record SetGroupAction(FilterGroup Group);
    public record MoveGroupToIndexAction(Guid GroupID, int Index);
    public record RemoveGroupAction(Guid GroupID);
    public record CloneGroupAction(Guid GroupID);

    public record SetBlockAction(Guid GroupID, FilterBlock Block);
    public record MoveBlockAction(Guid GroupID, Guid BlockID, int Index);
    public record RemoveBlockAction(Guid GroupID, Guid BlockID);
    public record CloneBlockAction(Guid GroupID, Guid BlockID);

    public record SetRuleAction(Guid GroupID, Guid BlockID, FilterRule Rule);
    public record MoveRuleAction(Guid GroupID, Guid BlockID, Guid RuleID, int Index);
    public record RemoveRuleAction(Guid GroupID, Guid BlockID, Guid RuleID);
    public record CloneRuleAction(Guid GroupID, Guid BlockID, Guid RuleID);

    public record ExportFilterAction;
    public record ExportFilterSuccessAction;
    public record ImportFilterAction(string FileContent);
    public record ImportFilterSuccessAction(Filter RootFilter);
    public record ExportGroupAction(Guid GroupID);
    public record ExportGroupSuccessAction;
    public record ImportGroupAction(string FileContent);
    public record ImportGroupSuccessAction;
    public record SaveFilterAction;
    public record SaveFilterSuccessAction;
    public record MergeGroupAndColorsAction(FilterGroup Group, List<FilterColorExport> ColorList);

    [ReducerMethod]
    public static FilterFeatureState ReduceSetColor(FilterFeatureState state, SetColorAction action)
    {
        List<FilterColor> updatedList = state.Filter.CustomColorCollection.GetValueOrDefault(action.Type, []).ToList();
        updatedList.SetItemByKey(action.Color, c => c.ID);

        return state with
        {
            Filter = state.Filter with
            {
                CustomColorCollection = new(state.Filter.CustomColorCollection) { [action.Type] = updatedList }
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceRemoveColor(FilterFeatureState state, RemoveColorAction action)
    {
        IReadOnlyList<FilterColor> listCurrent = state.Filter.CustomColorCollection.GetValueOrDefault(action.Type, []);
        return state with
        {
            Filter = state.Filter with
            {
                CustomColorCollection = new(state.Filter.CustomColorCollection) { [action.Type] = listCurrent.Where(c => c.ID != action.ColorID).ToList() }
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceSetGroup(FilterFeatureState state, SetGroupAction action)
    {
        List<FilterGroup> currentGroups = state.Filter.GroupList.ToList();
        currentGroups.SetItemByKey(action.Group, g => g.ID);

        return state with { Filter = state.Filter with { GroupList = currentGroups } };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceMoveGroupToIndex(FilterFeatureState state, MoveGroupToIndexAction action)
    {
        if (ModifyList(state.Filter.GroupList, g => g.ID == action.GroupID, action.Index, out List<FilterGroup> updated))
        {
            return state with { Filter = state.Filter with { GroupList = updated } };
        }

        return state;
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceRemoveGroup(FilterFeatureState state, RemoveGroupAction action)
    {
        return state with
        {
            Filter = state.Filter with { GroupList = state.Filter.GroupList.Where(g => g.ID != action.GroupID).ToList() }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceCloneGroup(FilterFeatureState state, CloneGroupAction action)
    {
        int index = state.Filter.GroupList.FindIndex(g => g.ID == action.GroupID);
        if (index == -1) return state;

        FilterGroup clonedGroup = state.Filter.GroupList[index] with
        {
            ID = Guid.NewGuid(),
            BlockList = [.. state.Filter.GroupList[index].BlockList.Select(b => b with { ID = Guid.NewGuid(), RuleList = [.. b.RuleList.Select(r => r with { ID = Guid.NewGuid() })] })]
        };

        return state with { Filter = state.Filter with { GroupList = state.Filter.GroupList.ToList().Step(l => l.Insert(index + 1, clonedGroup)) } };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceSetBlock(FilterFeatureState state, SetBlockAction action)
    {
        return UpdateGroupInState(state, action.GroupID, g => g with { BlockList = g.BlockList.ToList().SetItemByKey(action.Block, b => b.ID) });
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceMoveBlock(FilterFeatureState state, MoveBlockAction action)
    {
        return UpdateGroupInState(state, action.GroupID, g => ModifyList(g.BlockList, b => b.ID == action.BlockID, action.Index, out List<FilterBlock> updated) ? g with { BlockList = updated } : g);
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceRemoveBlock(FilterFeatureState state, RemoveBlockAction action)
    {
        return UpdateGroupInState(state, action.GroupID, g => g with { BlockList = g.RemoveBlock(action.BlockID) });
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceCloneBlock(FilterFeatureState state, CloneBlockAction action)
    {
        return UpdateGroupInState(state, action.GroupID, g =>
        {
            int index = g.BlockList.FindIndex(b => b.ID == action.BlockID);
            if (index == -1) return g;

            FilterBlock clonedBlock = g.BlockList[index] with { ID = Guid.NewGuid(), RuleList = [.. g.BlockList[index].RuleList.Select(r => r with { ID = Guid.NewGuid() })] };
            return g with { BlockList = g.BlockList.ToList().Step(l => l.Insert(index + 1, clonedBlock)) };
        });
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceSetRule(FilterFeatureState state, SetRuleAction action)
    {
        return UpdateGroupInState(state, action.GroupID, g => UpdateBlockInList(g, action.BlockID, b => b with { RuleList = b.RuleList.ToList().SetItemByKey(action.Rule, r => r.ID) }));
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceMoveRule(FilterFeatureState state, MoveRuleAction action)
    {
        return UpdateGroupInState(state, action.GroupID, g => UpdateBlockInList(g, action.BlockID, b => b with { RuleList = b.MoveRule(action.RuleID, action.Index) }));
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceRemoveRule(FilterFeatureState state, RemoveRuleAction action)
    {
        return UpdateGroupInState(state, action.GroupID, g => UpdateBlockInList(g, action.BlockID, b => b with { RuleList = b.RemoveRule(action.RuleID) }));
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceCloneRule(FilterFeatureState state, CloneRuleAction action)
    {
        return UpdateGroupInState(state, action.GroupID, g => UpdateBlockInList(g, action.BlockID, b =>
        {
            int index = b.RuleList.FindIndex(r => r.ID == action.RuleID);
            if (index == -1) return b;
            return b with { RuleList = b.RuleList.ToList().Step(l => l.Insert(index + 1, b.RuleList[index] with { ID = Guid.NewGuid() })) };
        }));
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceExportFilter(FilterFeatureState state, ExportFilterAction action)
    {
        return state with { IsLoading = true };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceExportFilterSuccess(FilterFeatureState state, ExportFilterSuccessAction action)
    {
        return state with { IsLoading = false };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceImportFilter(FilterFeatureState state, ImportFilterAction action)
    {
        return state with { IsLoading = true };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceImportFilterSuccess(FilterFeatureState state, ImportFilterSuccessAction action)
    {
        return state with { IsLoading = false, Filter = action.RootFilter };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceImportGroup(FilterFeatureState state, ImportGroupAction action)
    {
        return state with { IsLoading = true };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceImportGroupSuccess(FilterFeatureState state, ImportGroupSuccessAction action)
    {
        return state with { IsLoading = false };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceExportGroup(FilterFeatureState state, ExportGroupAction action)
    {
        return state with { IsLoading = true };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceExportGroupSuccess(FilterFeatureState state, ExportGroupSuccessAction action)
    {
        return state with { IsLoading = false };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceSaveFilter(FilterFeatureState state, SaveFilterAction action)
    {
        return state with { IsLoading = true };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceSaveFilterSuccess(FilterFeatureState state, SaveFilterSuccessAction action)
    {
        return state with { IsLoading = false };
    }

    [ReducerMethod]
    public static FilterFeatureState OnMergeGroupAndColors(FilterFeatureState state, MergeGroupAndColorsAction action)
    {
        Dictionary<FilterColorTypeEnum, List<FilterColor>> collectionColor = state.Filter.CustomColorCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());
        Dictionary<FilterColor, FilterColor> map = [];

        foreach (FilterColorExport incoming in action.ColorList)
        {
            if (!collectionColor.TryGetValue(incoming.Type, out List<FilterColor>? list))
            {
                list = [];
                collectionColor[incoming.Type] = list;
            }

            FilterColor? match = list.FirstOrDefault(color => color.ValueEquals(incoming.Color));
            if (match is not null)
            {
                map[incoming.Color] = match;
            }
            else
            {
                map[incoming.Color] = incoming.Color with { ID = Guid.NewGuid() };
                list.Add(map[incoming.Color]);
            }
        }

        List<FilterBlock> listUpdated = [.. action.Group.BlockList.Select(block => block with { RuleList = [..block.RuleList.Select(rule => MapRuleColorReferences(map, rule))] })];

        return state with
        {
            Filter = state.Filter with
            {
                GroupList = [.. state.Filter.GroupList, action.Group with { BlockList = listUpdated }],
                CustomColorCollection = collectionColor.ToDictionary(pair => pair.Key, IReadOnlyList<FilterColor> (pair) => pair.Value)
            }
        };
    }

    private static FilterFeatureState UpdateGroupInState(FilterFeatureState state, Guid groupId, Func<FilterGroup, FilterGroup> updateLogic)
    {
        int index = state.Filter.GroupList.FindIndex(g => g.ID == groupId);
        if (index == -1) return state;

        List<FilterGroup> listGroup = state.Filter.GroupList.ToList();
        listGroup[index] = updateLogic(listGroup[index]);

        return state with { Filter = state.Filter with { GroupList = listGroup } };
    }

    private static FilterGroup UpdateBlockInList(FilterGroup group, Guid blockId, Func<FilterBlock, FilterBlock> updateLogic)
    {
        int index = group.BlockList.FindIndex(b => b.ID == blockId);
        if (index == -1) return group;

        List<FilterBlock> listBlock = group.BlockList.ToList();
        listBlock[index] = updateLogic(listBlock[index]);

        return group with { BlockList = listBlock };
    }

    private static bool ModifyList<T>(IReadOnlyList<T> source, Func<T, bool> predicate, int targetIndex, out List<T> result)
    {
        result = source.ToList();
        int sourceIndex = result.FindIndex(new(predicate));
        if (sourceIndex == -1 || targetIndex < 0 || targetIndex >= result.Count || sourceIndex == targetIndex) return false;

        T item = result[sourceIndex];
        result.RemoveAt(sourceIndex);
        result.Insert(targetIndex, item);
        return true;
    }

    private static T Step<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }

    private static FilterRule MapRuleColorReferences(Dictionary<FilterColor, FilterColor> map, FilterRule rule)
    {
        if (rule is FilterRule<FilterColor?> { Value: { } color } ruleColor && map.TryGetValue(color, out FilterColor? colorMatched))
        {
            return ruleColor with { Value = colorMatched };
        }

        return rule;
    }
}
