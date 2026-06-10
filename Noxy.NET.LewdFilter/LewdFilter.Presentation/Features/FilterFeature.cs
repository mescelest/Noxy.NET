using LewdFilter.Domain.Enums;
using LewdFilter.Domain.Models;
using LewdFilter.Domain.Services;

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
            [ColorTypeEnum.Border] = [FilterColor.DefaultBorder],
            [ColorTypeEnum.Text] = [FilterColor.DefaultText],
            [ColorTypeEnum.Background] = [FilterColor.DefaultBackground],
        }
    };

    public bool IsLoading { get; init; }
}

public class FilterFeatureReducers
{
    public record AddColorAction(ColorTypeEnum Type, FilterColor Color);

    public record EditColorAction(ColorTypeEnum Type, FilterColor Color);

    public record RemoveColorAction(ColorTypeEnum Type, Guid ColorID);

    public record AddGroupAction(FilterGroup Group);

    public record EditGroupAction(FilterGroup Group);

    public record RemoveGroupAction(Guid GroupID);

    public record AddBlockAction(Guid GroupID, FilterBlock Block);

    public record EditBlockAction(Guid GroupID, FilterBlock Block);

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
            Filter = new()
            {
                ID = state.Filter.ID,
                Name = state.Filter.Name,
                GroupList = state.Filter.GroupList,
                CustomColorCollection = new(state.Filter.CustomColorCollection) { [action.Type] = [..listCurrent, action.Color] }
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceEditColor(FilterFeatureState state, EditColorAction action)
    {
        List<FilterColor> listCurrent = state.Filter.CustomColorCollection.GetValueOrDefault(action.Type, []);

        return state with
        {
            Filter = new()
            {
                ID = state.Filter.ID,
                Name = state.Filter.Name,
                GroupList = state.Filter.GroupList,
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
            Filter = new()
            {
                ID = state.Filter.ID,
                Name = state.Filter.Name,
                GroupList = state.Filter.GroupList,
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
            Filter = new()
            {
                ID = state.Filter.ID,
                Name = state.Filter.Name,
                CustomColorCollection = state.Filter.CustomColorCollection,
                GroupList = [..state.Filter.GroupList, action.Group]
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceEditGroup(FilterFeatureState state, EditGroupAction action)
    {
        return state with
        {
            Filter = new()
            {
                ID = state.Filter.ID,
                Name = state.Filter.Name,
                CustomColorCollection = state.Filter.CustomColorCollection,
                GroupList = state.Filter.GroupList.Select(g => g.ID == action.Group.ID ? action.Group : g).ToList()
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceRemoveGroup(FilterFeatureState state, RemoveGroupAction action)
    {
        return state with
        {
            Filter = new()
            {
                ID = state.Filter.ID,
                Name = state.Filter.Name,
                CustomColorCollection = state.Filter.CustomColorCollection,
                GroupList = state.Filter.GroupList.Where(g => g.ID != action.GroupID).ToList()
            }
        };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceAddBlock(FilterFeatureState state, AddBlockAction action)
    {
        if (!state.Filter.TryGetGroup(action.GroupID, out FilterGroup? group)) return state;

        List<FilterBlock> listBlockUpdated = group.AddBlock(action.Block);
        FilterGroup groupUpdated = new() { ID = group.ID, Name = group.Name, BlockList = listBlockUpdated };
        List<FilterGroup> listGroupUpdated = state.Filter.GroupList.Select(g => g.ID == action.GroupID ? groupUpdated : g).ToList();

        Filter filterCloned = state.Filter.Clone();
        filterCloned.GroupList = listGroupUpdated;

        return state with { Filter = filterCloned };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceEditBlock(FilterFeatureState state, EditBlockAction action)
    {
        if (!state.Filter.TryGetGroup(action.GroupID, out FilterGroup? group) || !group.TryReplaceBlock(action.Block, out List<FilterBlock>? listBlockUpdated)) return state;

        FilterGroup groupUpdated = new() { ID = group.ID, Name = group.Name, BlockList = listBlockUpdated };
        List<FilterGroup> listGroupUpdated = state.Filter.GroupList.Select(g => g.ID == action.GroupID ? groupUpdated : g).ToList();

        Filter filterCloned = state.Filter.Clone();
        filterCloned.GroupList = listGroupUpdated;

        return state with { Filter = filterCloned };
    }

    [ReducerMethod]
    public static FilterFeatureState ReduceRemoveBlock(FilterFeatureState state, RemoveBlockAction action)
    {
        if (!state.Filter.TryGetGroup(action.GroupID, out FilterGroup? group)) return state;

        List<FilterBlock> listBlockUpdated = group.RemoveBlock(action.BlockID);
        FilterGroup groupUpdated = new() { ID = group.ID, Name = group.Name, BlockList = listBlockUpdated };
        List<FilterGroup> listGroupUpdated = state.Filter.GroupList.Select(g => g.ID == action.GroupID ? groupUpdated : g).ToList();

        Filter filterCloned = state.Filter.Clone();
        filterCloned.GroupList = listGroupUpdated;

        return state with { Filter = filterCloned };
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
            await js.InvokeVoidAsync("downloadFileFromStream", fileName, jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting configuration JSON: {ex.Message}");
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

            if (importedFilter != null)
            {
                dispatcher.Dispatch(new FilterFeatureReducers.ImportFilterSuccessAction(importedFilter));
            }
            else
            {
                Console.WriteLine("Import failed: Deserialization returned a null object reference.");
                dispatcher.Dispatch(new FilterFeatureReducers.ImportFilterSuccessAction(state.Value.Filter));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error parsing imported JSON profile: {ex.Message}");
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
            string compiledOutput = FilterCompilerService.Compile(payload);
            string fileName = $"{(string.IsNullOrWhiteSpace(payload.Name) ? DefaultFileName : payload.Name)}.filter";
            await js.InvokeVoidAsync("downloadFileFromStream", fileName, compiledOutput);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FilterCompilerService processing failed: {ex.Message}");
        }
        finally
        {
            dispatcher.Dispatch(new FilterFeatureReducers.SaveFilterSuccessAction());
        }
    }
}
