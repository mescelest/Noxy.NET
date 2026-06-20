using System.Text;
using Fluxor;
using LewdFilter.Domain.Enums;
using LewdFilter.Domain.Models;
using LewdFilter.Domain.Services;
using Microsoft.JSInterop;

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
}

public class FilterFeatureReducers
{
    public record AddColorAction(FilterColorTypeEnum Type, FilterColor Color);

    public record EditColorAction(FilterColorTypeEnum Type, FilterColor Color);

    public record RemoveColorAction(FilterColorTypeEnum Type, Guid ColorID);

    public record AddGroupAction(FilterGroup Group);

    public record MoveGroupToIndexAction(Guid GroupID, int Index);

    public record EditGroupAction(FilterGroup Group);

    public record RemoveGroupAction(Guid GroupID);

    public record AddBlockAction(Guid GroupID, FilterBlock Block);

    public record EditBlockAction(Guid GroupID, FilterBlock Block);

    public record MoveBlockToIndexAction(Guid GroupID, Guid BlockID, int Index);

    public record RemoveBlockAction(Guid GroupID, Guid BlockID);

    public record ExportFilterAction;

    public record ExportFilterSuccessAction;

    public record ImportFilterAction(string FileContent);

    public record ImportFilterSuccessAction(Filter RootFilter);

    public record SaveFilterAction;

    public record SaveFilterSuccessAction;

    [ReducerMethod]
    public static FilterFeatureState ReduceAddColor(FilterFeatureState state, AddColorAction action)
    {
        List<FilterColor> listCurrent = state.Filter.CustomColorCollection.GetValueOrDefault(action.Type, []);

        return state with
        {
            Filter = state.Filter with
            {
                CustomColorCollection = new(state.Filter.CustomColorCollection)
                {
                    [action.Type] = [.. listCurrent, action.Color]
                }
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceEditColor(FilterFeatureState state, EditColorAction action)
    {
        List<FilterColor> listCurrent = state.Filter.CustomColorCollection.GetValueOrDefault(action.Type, []);

        return state with
        {
            Filter = state.Filter with
            {
                CustomColorCollection = new(state.Filter.CustomColorCollection)
                {
                    [action.Type] = listCurrent.Select(c => c.ID == action.Color.ID ? action.Color : c).ToList()
                }
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
                CustomColorCollection = new(state.Filter.CustomColorCollection)
                {
                    [action.Type] = listCurrent.Where(c => c.ID != action.ColorID).ToList()
                }
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceAddGroup(FilterFeatureState state, AddGroupAction action)
    {
        return state with
        {
            Filter = state.Filter with { GroupList = [.. state.Filter.GroupList, action.Group] }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceEditGroup(FilterFeatureState state, EditGroupAction action)
    {
        return state with
        {
            Filter = state.Filter with
            {
                GroupList = state.Filter.GroupList.Select(g => g.ID == action.Group.ID ? action.Group : g).ToList()
            }
        };
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

        return state with
        {
            Filter = state.Filter with { GroupList = listGroup }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceRemoveGroup(FilterFeatureState state, RemoveGroupAction action)
    {
        return state with
        {
            Filter = state.Filter with
            {
                GroupList = state.Filter.GroupList.Where(g => g.ID != action.GroupID).ToList()
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceAddBlock(FilterFeatureState state, AddBlockAction action)
    {
        FilterGroup? group = state.Filter.GroupList.FirstOrDefault(g => g.ID == action.GroupID);
        if (group == null) return state;

        List<FilterBlock> listBlock = group.AddBlock(action.Block);
        FilterGroup groupCurrent = group with { BlockList = listBlock };

        return state with
        {
            Filter = state.Filter with
            {
                GroupList = state.Filter.GroupList.Select(g => g.ID == action.GroupID ? groupCurrent : g).ToList()
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceEditBlock(FilterFeatureState state, EditBlockAction action)
    {
        FilterGroup? group = state.Filter.GroupList.FirstOrDefault(g => g.ID == action.GroupID);
        if (group == null) return state;

        List<FilterBlock> listBlock = group.BlockList.Select(b => b.ID == action.Block.ID ? action.Block : b).ToList();
        FilterGroup groupCurrent = group with { BlockList = listBlock };

        return state with
        {
            Filter = state.Filter with
            {
                GroupList = state.Filter.GroupList.Select(g => g.ID == action.GroupID ? groupCurrent : g).ToList()
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceMoveBlockToIndex(FilterFeatureState state, MoveBlockToIndexAction action)
    {
        FilterGroup? group = state.Filter.GroupList.FirstOrDefault(g => g.ID == action.GroupID);
        if (group == null) return state;

        List<FilterBlock> listBlock = group.BlockList.ToList();
        int index = listBlock.FindIndex(b => b.ID == action.BlockID);
        if (index == -1 || action.Index < 0 || action.Index >= listBlock.Count || index == action.Index) return state;

        FilterBlock item = listBlock[index];
        listBlock.RemoveAt(index);
        listBlock.Insert(action.Index, item);

        FilterGroup groupCurrent = group with { BlockList = listBlock };

        return state with
        {
            Filter = state.Filter with
            {
                GroupList = state.Filter.GroupList.Select(g => g.ID == action.GroupID ? groupCurrent : g).ToList()
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceRemoveBlock(FilterFeatureState state, RemoveBlockAction action)
    {
        FilterGroup? group = state.Filter.GroupList.FirstOrDefault(g => g.ID == action.GroupID);
        if (group == null) return state;

        List<FilterBlock> listBlock = group.RemoveBlock(action.BlockID);
        FilterGroup groupCurrent = group with { BlockList = listBlock };

        return state with
        {
            Filter = state.Filter with
            {
                GroupList = state.Filter.GroupList.Select(g => g.ID == action.GroupID ? groupCurrent : g).ToList()
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceExport(FilterFeatureState state, ExportFilterAction action) => state with { IsLoading = true };

    [ReducerMethod]
    public static FilterFeatureState ReduceImport(FilterFeatureState state, ImportFilterAction action) => state with { IsLoading = true };

    [ReducerMethod]
    public static FilterFeatureState ReduceSave(FilterFeatureState state, SaveFilterAction action) => state with { IsLoading = true };

    [ReducerMethod]
    public static FilterFeatureState ReduceExportSuccess(FilterFeatureState state, ExportFilterSuccessAction action) => state with { IsLoading = false };

    [ReducerMethod]
    public static FilterFeatureState ReduceImportSuccess(FilterFeatureState state, ImportFilterSuccessAction action) => state with { IsLoading = false, Filter = action.RootFilter };

    [ReducerMethod]
    public static FilterFeatureState ReduceSaveSuccess(FilterFeatureState state, SaveFilterSuccessAction action) => state with { IsLoading = false };
}

public class FilterFeatureEffects(FilterCompilerService compiler, FilterStorageService storageService, IState<FilterFeatureState> state, IJSRuntime js)
{
    private const string DefaultFileName = "filter";

    [EffectMethod]
    public async Task HandleExportFilterAction(FilterFeatureReducers.ExportFilterAction action, IDispatcher dispatcher)
    {
        try
        {
            Filter filterData = state.Value.Filter;
            string jsonString = storageService.SaveFilterToJson(filterData);
            string fileName = $"{(string.IsNullOrWhiteSpace(filterData.Name) ? DefaultFileName : filterData.Name)}.json";

            byte[] fileBytes = Encoding.UTF8.GetBytes(jsonString);
            using MemoryStream stream = new(fileBytes);
            using DotNetStreamReference streamRef = new(stream);

            await js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
        }
        catch (Exception)
        {
            // ignored
        }
        finally
        {
            dispatcher.Dispatch(new FilterFeatureReducers.ExportFilterSuccessAction());
        }
    }

    [EffectMethod]
    public Task HandleImportFilterAction(FilterFeatureReducers.ImportFilterAction action, IDispatcher dispatcher)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(action.FileContent)) return Task.CompletedTask;
            Filter? importedFilter = storageService.LoadFilterFromJson(action.FileContent);

            dispatcher.Dispatch(importedFilter != null
                ? new FilterFeatureReducers.ImportFilterSuccessAction(importedFilter)
                : new FilterFeatureReducers.ImportFilterSuccessAction(state.Value.Filter)
            );
        }
        catch (Exception)
        {
            dispatcher.Dispatch(new FilterFeatureReducers.ImportFilterSuccessAction(state.Value.Filter));
        }

        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task HandleSaveFilterAction(FilterFeatureReducers.SaveFilterAction action, IDispatcher dispatcher)
    {
        try
        {
            Filter payload = state.Value.Filter;
            string compiledOutput = compiler.Compile(payload);
            string fileName = $"{(string.IsNullOrWhiteSpace(payload.Name) ? DefaultFileName : payload.Name)}.filter";

            byte[] fileBytes = Encoding.UTF8.GetBytes(compiledOutput);
            using MemoryStream stream = new(fileBytes);
            using DotNetStreamReference streamRef = new(stream);

            await js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
        }
        catch (Exception)
        {
            // ignored
        }
        finally
        {
            dispatcher.Dispatch(new FilterFeatureReducers.SaveFilterSuccessAction());
        }
    }
}
