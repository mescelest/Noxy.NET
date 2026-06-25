using System.Text;
using Fluxor;
using LewdFilter.Domain.Models;
using LewdFilter.Domain.Services;
using Microsoft.JSInterop;
using Noxy.NET.Extensions;

namespace LewdFilter.Presentation.Features;

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
            FilterGroup targetGroup = FilterFeatureState.SelectGroup(action.GroupID)(state.Value);
            if (targetGroup == FilterGroup.Default) return new FilterFeatureReducers.ExportGroupSuccessAction();

            HashSet<Guid> listColorID = [];
            foreach (FilterBlock block in targetGroup.BlockList)
            {
                foreach (FilterRule rule in block.RuleList)
                {
                    if (rule is FilterRule<FilterColor?> { Value: { } color })
                    {
                        listColorID.Add(color.ID);
                    }
                }
            }

            List<FilterColorExport> listColor = [.. from kvp in state.Value.Filter.CustomColorCollection from color in kvp.Value where listColorID.Contains(color.ID) select new FilterColorExport(kvp.Key, color)];
            FilterGroupExport package = new(targetGroup, listColor);
            string json = storage.SerializeGroupExport(package);

            await DownloadAsync($"{GetFilterName()}.{GetGroupName(targetGroup)}.{JsonExtension}", json);
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
