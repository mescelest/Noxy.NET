using System.Collections.Immutable;
using System.Text;
using Fluxor;
using LewdFilter.Domain.Enums;
using LewdFilter.Domain.Models;
using LewdFilter.Domain.Services;
using Microsoft.JSInterop;
using Noxy.NET.Extensions;

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

    public static Func<FilterFeatureState, FilterGroup> SelectGroup(Guid groupId) => state => state.Filter.GroupList.FirstOrDefault(g => g.ID == groupId) ?? FilterGroup.Default;

    public static Func<FilterFeatureState, string> SelectGroupName(Guid groupId) => state => SelectGroup(groupId)(state).Name;

    public static Func<FilterFeatureState, ImmutableArray<FilterGroup>> SelectGroupList() => state => [.. state.Filter.GroupList];

    public static Func<FilterFeatureState, ImmutableArray<Guid>> SelectGroupIDList() => state => [.. SelectGroupList()(state).Select(g => g.ID)];

    public static Func<FilterFeatureState, FilterBlock> SelectBlock(Guid groupId, Guid blockId) => state => SelectGroup(groupId)(state).BlockList.FirstOrDefault(b => b.ID == blockId) ?? FilterBlock.Default;

    public static Func<FilterFeatureState, ImmutableArray<Guid>> SelectBlockIDList(Guid groupId) => state => [.. SelectGroup(groupId)(state).BlockList.Select(b => b.ID)];

    public static Func<FilterFeatureState, int> SelectBlockBaseTypeCount(Guid groupId, Guid blockId) => state => SelectBlock(groupId, blockId)(state).BaseTypeList.Count;

    public static Func<FilterFeatureState, ImmutableDictionary<FilterColorTypeEnum, List<FilterColor>>> SelectCustomColorList() => state => state.Filter.CustomColorCollection.ToImmutableDictionary();

    public static Func<FilterFeatureState, ImmutableArray<FilterColor>> SelectColorListByTab(FilterColorTypeEnum tab) => state => state.Filter.CustomColorCollection.TryGetValue(tab, out List<FilterColor>? colors) ? [.. colors] : [];

    public static Func<FilterFeatureState, string> SelectPreviewStyle(Guid groupId, Guid blockId) => state =>
    {
        FilterBlock block = SelectBlock(groupId, blockId)(state);

        string textHex = (block.TextColor ?? FilterColor.DefaultText).ToHex();
        string bgHex = (block.BackgroundColor ?? FilterColor.DefaultBackground).ToHex();
        string borderStyle = (block.BorderColor ?? FilterColor.DefaultBorder).ToHex();
        string fontSize = $"{(block.FontSize ?? 32) / 32.0:0.00}rem";

        return $"color: {textHex}; background-color: {bgHex}; border: 1px solid {borderStyle}; font-size: {fontSize};";
    };
}

public class FilterFeatureReducers
{
    public record SetColorAction(FilterColorTypeEnum Type, FilterColor Color);
    public record RemoveColorAction(FilterColorTypeEnum Type, Guid ColorID);

    public record SetGroupAction(FilterGroup Group);
    public record MoveGroupToIndexAction(Guid GroupID, int Index);
    public record RemoveGroupAction(Guid GroupID);

    public record SetBlockAction(Guid GroupID, FilterBlock Block);
    public record MoveBlockAction(Guid GroupID, Guid BlockID, int Index);
    public record RemoveBlockAction(Guid GroupID, Guid BlockID);

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
        List<FilterColor> listCurrent = state.Filter.CustomColorCollection.GetValueOrDefault(action.Type, []);
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
        List<FilterGroup> listGroup = state.Filter.GroupList.ToList();
        int index = listGroup.FindIndex(g => g.ID == action.GroupID);
        if (index == -1 || action.Index < 0 || action.Index >= listGroup.Count || index == action.Index) return state;

        FilterGroup item = listGroup[index];
        listGroup.RemoveAt(index);
        listGroup.Insert(action.Index, item);

        return state with { Filter = state.Filter with { GroupList = listGroup } };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceRemoveGroup(FilterFeatureState state, RemoveGroupAction action) => state with
    {
        Filter = state.Filter with { GroupList = state.Filter.GroupList.Where(g => g.ID != action.GroupID).ToList() }
    };

    [ReducerMethod]
    public static FilterFeatureState ReduceSetBlock(FilterFeatureState state, SetBlockAction action) =>
        UpdateGroupInState(state, action.GroupID, group =>
        {
            List<FilterBlock> currentBlocks = group.BlockList.ToList();
            currentBlocks.SetItemByKey(action.Block, b => b.ID);

            return group with { BlockList = currentBlocks };
        });

    [ReducerMethod]
    public static FilterFeatureState ReduceMoveBlock(FilterFeatureState state, MoveBlockAction action) =>
        UpdateGroupInState(state, action.GroupID, g =>
        {
            List<FilterBlock> listBlock = g.BlockList.ToList();
            int index = listBlock.FindIndex(b => b.ID == action.BlockID);
            if (index == -1 || action.Index < 0 || action.Index >= listBlock.Count || index == action.Index) return g;

            FilterBlock item = listBlock[index];
            listBlock.RemoveAt(index);
            listBlock.Insert(action.Index, item);
            return g with { BlockList = listBlock };
        });

    [ReducerMethod]
    public static FilterFeatureState ReduceRemoveBlock(FilterFeatureState state, RemoveBlockAction action) =>
        UpdateGroupInState(state, action.GroupID, g => g with { BlockList = g.RemoveBlock(action.BlockID) });

    [ReducerMethod]
    public static FilterFeatureState ReduceExportFilter(FilterFeatureState state, ExportFilterAction action) => state with { IsLoading = true };

    [ReducerMethod]
    public static FilterFeatureState ReduceExportFilterSuccess(FilterFeatureState state, ExportFilterSuccessAction action) => state with { IsLoading = false };

    [ReducerMethod]
    public static FilterFeatureState ReduceImportFilter(FilterFeatureState state, ImportFilterAction action) => state with { IsLoading = true };

    [ReducerMethod]
    public static FilterFeatureState ReduceImportFilterSuccess(FilterFeatureState state, ImportFilterSuccessAction action) => state with { IsLoading = false, Filter = action.RootFilter };

    [ReducerMethod]
    public static FilterFeatureState ReduceImportGroup(FilterFeatureState state, ImportGroupAction action) => state with { IsLoading = true };

    [ReducerMethod]
    public static FilterFeatureState ReduceImportGroupSuccess(FilterFeatureState state, ImportGroupSuccessAction action) => state with { IsLoading = false };

    [ReducerMethod]
    public static FilterFeatureState ReduceExportGroup(FilterFeatureState state, ExportGroupAction action) => state with { IsLoading = true };

    [ReducerMethod]
    public static FilterFeatureState ReduceExportGroupSuccess(FilterFeatureState state, ExportGroupSuccessAction action) => state with { IsLoading = false };

    [ReducerMethod]
    public static FilterFeatureState ReduceSaveFilter(FilterFeatureState state, SaveFilterAction action) => state with { IsLoading = true };

    [ReducerMethod]
    public static FilterFeatureState ReduceSaveFilterSuccess(FilterFeatureState state, SaveFilterSuccessAction action) => state with { IsLoading = false };

    [ReducerMethod]
    public static FilterFeatureState OnMergeGroupAndColors(FilterFeatureState state, MergeGroupAndColorsAction action)
    {
        Dictionary<FilterColorTypeEnum, List<FilterColor>> collectionColor = new(state.Filter.CustomColorCollection.ToDictionary(kvp => kvp.Key, kvp => new List<FilterColor>(kvp.Value)));
        Dictionary<FilterColor, FilterColor> referenceMap = [];

        foreach (FilterColorExport incoming in action.ColorList)
        {
            if (!collectionColor.TryGetValue(incoming.Type, out List<FilterColor>? list))
            {
                list = [];
                collectionColor[incoming.Type] = list;
            }

            FilterColor? match = list.FirstOrDefault(c => c.ValueEquals(incoming.Color));
            if (match is not null)
            {
                referenceMap[incoming.Color] = match;
            }
            else
            {
                FilterColor absoluteNewColor = incoming.Color with { ID = Guid.NewGuid() };
                list.Add(absoluteNewColor);
                referenceMap[incoming.Color] = absoluteNewColor;
            }
        }

        List<FilterBlock> listUpdated =
        [
            .. action.Group.BlockList.Select(block => block with
            {
                TextColor = block.TextColor is not null && referenceMap.TryGetValue(block.TextColor, out FilterColor? newText) ? newText : block.TextColor,
                BackgroundColor = block.BackgroundColor is not null && referenceMap.TryGetValue(block.BackgroundColor, out FilterColor? newBg) ? newBg : block.BackgroundColor,
                BorderColor = block.BorderColor is not null && referenceMap.TryGetValue(block.BorderColor, out FilterColor? newBorder) ? newBorder : block.BorderColor
            })
        ];

        FilterGroup group = action.Group with { BlockList = listUpdated };
        return state with { Filter = state.Filter with { GroupList = [.. state.Filter.GroupList, group], CustomColorCollection = collectionColor } };
    }

    private static FilterFeatureState UpdateGroupInState(FilterFeatureState state, Guid groupId, Func<FilterGroup, FilterGroup> updateLogic)
    {
        int index = state.Filter.GroupList.FindIndex(g => g.ID == groupId);
        if (index == -1) return state;

        List<FilterGroup> updatedGroups = state.Filter.GroupList.ToList();
        updatedGroups[index] = updateLogic(updatedGroups[index]);

        return state with { Filter = state.Filter with { GroupList = updatedGroups } };
    }
}

public class FilterFeatureEffects(FilterCompilerService compiler, FilterStorageService storage, IState<FilterFeatureState> state, IDispatcher dispatcher, IJSRuntime js)
{
    private const string DefaultFilterName = "filter";
    private const string DefaultGroupName = "group";

    private const string DownloadModulePath = "./js/download.js";
    private const string DownloadIdentifier = "download";
    private const string JsonExtension = "json";
    private const string FilterExtension = "filter";

    [EffectMethod]
    public Task HandleExportGroupAction(FilterFeatureReducers.ExportGroupAction action, IDispatcher _)
    {
        return RunSafeAsync(async () =>
        {
            FilterGroup group = FilterFeatureState.SelectGroup(action.GroupID)(state.Value);
            if (group == FilterGroup.Default) return new FilterFeatureReducers.ExportGroupSuccessAction();

            HashSet<Guid> listColorID = [];
            foreach (FilterBlock block in group.BlockList)
            {
                if (block.TextColor is not null) listColorID.Add(block.TextColor.ID);
                if (block.BackgroundColor is not null) listColorID.Add(block.BackgroundColor.ID);
                if (block.BorderColor is not null) listColorID.Add(block.BorderColor.ID);
            }

            List<FilterColorExport> listColor = [.. from kvp in state.Value.Filter.CustomColorCollection from color in kvp.Value where listColorID.Contains(color.ID) select new FilterColorExport(kvp.Key, color)];

            FilterGroupExport package = new(group, listColor);
            string json = storage.SerializeGroupExport(package);

            await DownloadAsync($"{GetFilterName()}.{GetGroupName(group)}.{JsonExtension}", json);
            return new FilterFeatureReducers.ExportGroupSuccessAction();
        });
    }

    [EffectMethod]
    public Task HandleImportGroupAction(FilterFeatureReducers.ImportGroupAction action, IDispatcher _)
    {
        return RunSafeAsync(() =>
        {
            if (string.IsNullOrWhiteSpace(action.FileContent)) return Task.FromResult<object>(new FilterFeatureReducers.ImportGroupSuccessAction());

            FilterGroupExport? package = storage.DeserializeGroupImport(action.FileContent);
            if (package?.Group is null) return Task.FromResult<object>(new FilterFeatureReducers.ImportGroupSuccessAction());

            dispatcher.Dispatch(new FilterFeatureReducers.MergeGroupAndColorsAction(package.Group, package.ColorList));
            return Task.FromResult<object>(new FilterFeatureReducers.ImportGroupSuccessAction());
        });
    }

    [EffectMethod]
    public Task HandleExportFilterAction(FilterFeatureReducers.ExportFilterAction action, IDispatcher _)
    {
        return RunSafeAsync(async () =>
        {
            await DownloadAsync($"{GetFilterName()}.{JsonExtension}", storage.SaveFilterToJson(state.Value.Filter));
            return new FilterFeatureReducers.ExportFilterSuccessAction();
        });
    }

    [EffectMethod]
    public Task HandleImportFilterAction(FilterFeatureReducers.ImportFilterAction action, IDispatcher _)
    {
        return RunSafeAsync(() =>
        {
            Filter? imported = string.IsNullOrWhiteSpace(action.FileContent) ? null : storage.LoadFilterFromJson(action.FileContent);
            return Task.FromResult<object>(new FilterFeatureReducers.ImportFilterSuccessAction(imported ?? state.Value.Filter));
        });
    }

    [EffectMethod]
    public Task HandleSaveFilterAction(FilterFeatureReducers.SaveFilterAction action, IDispatcher _)
    {
        return RunSafeAsync(async () =>
        {
            await DownloadAsync($"{GetFilterName()}.{FilterExtension}", compiler.Compile(state.Value.Filter));
            return new FilterFeatureReducers.SaveFilterSuccessAction();
        });
    }

    private string GetFilterName()
    {
        return !string.IsNullOrWhiteSpace(state.Value.Filter.Name) ? state.Value.Filter.Name.SanitizeFileName() : DefaultFilterName;
    }

    private static string GetGroupName(FilterGroup group)
    {
        return !string.IsNullOrWhiteSpace(group.Name) ? group.Name.SanitizeFileName() : DefaultGroupName;
    }

    private async Task DownloadAsync(string fileName, string content)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(content));
        using DotNetStreamReference streamRef = new(stream);

        await using IJSObjectReference module = await js.InvokeAsync<IJSObjectReference>("import", DownloadModulePath);
        await module.InvokeVoidAsync(DownloadIdentifier, fileName, streamRef);
    }

    private async Task RunSafeAsync(Func<Task<object>> process)
    {
        object? successAction = null;
        try
        {
            successAction = await process();
        }
        catch (Exception ex)
        {
            // ignored
        }
        finally
        {
            if (successAction is not null) dispatcher.Dispatch(successAction);
        }
    }
}
