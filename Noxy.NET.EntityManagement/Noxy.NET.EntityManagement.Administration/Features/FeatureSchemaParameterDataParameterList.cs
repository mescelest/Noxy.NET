using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Requests.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Administration.Features;

[FeatureState]
public record FeatureSchemaParameterDataParameterListState
{
    public bool IsLoading { get; init; }
    public Dictionary<string, bool> IsLoadingPerKey { get; init; } = new();
    public Dictionary<string, IReadOnlyList<EntityDataParameter>> Cache { get; init; } = new();
    public string? ErrorMessage { get; init; }

    public bool IsKeyLoading(string schemaIdentifier) => IsLoadingPerKey.TryGetValue(schemaIdentifier, out bool isLoading) && isLoading;
}

public static class FeatureSchemaParameterDataParameterListReducers
{
    public record LoadDataParameterListAction(string SchemaIdentifier);

    public record AddDataParameterListAction(string SchemaIdentifier, EntityDataParameter Entity);

    public record RemoveDataParameterListAction(string SchemaIdentifier, Guid ID);

    public record DataParameterListResultAction(string SchemaIdentifier, IReadOnlyList<EntityDataParameter.Discriminator> Result);

    public record DataParameterListFailedAction(string SchemaIdentifier, string Error);

    [ReducerMethod]
    public static FeatureSchemaParameterDataParameterListState ReduceLoad(FeatureSchemaParameterDataParameterListState state, LoadDataParameterListAction action)
    {
        return state with
        {
            IsLoading = true,
            IsLoadingPerKey = new(state.IsLoadingPerKey) { [action.SchemaIdentifier] = true },
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static FeatureSchemaParameterDataParameterListState ReduceAdd(FeatureSchemaParameterDataParameterListState state, AddDataParameterListAction action)
    {
        Dictionary<string, IReadOnlyList<EntityDataParameter>> newCache = new(state.Cache);
        IReadOnlyList<EntityDataParameter> list = newCache.GetValueOrDefault(action.SchemaIdentifier) ?? [];
        newCache[action.SchemaIdentifier] = [.. list, action.Entity];

        return state with
        {
            Cache = newCache
        };
    }

    [ReducerMethod]
    public static FeatureSchemaParameterDataParameterListState ReduceRemove(FeatureSchemaParameterDataParameterListState state, RemoveDataParameterListAction action)
    {
        Dictionary<string, IReadOnlyList<EntityDataParameter>> newCache = new(state.Cache);
        IReadOnlyList<EntityDataParameter> list = newCache.GetValueOrDefault(action.SchemaIdentifier) ?? [];
        newCache[action.SchemaIdentifier] = list.Where(x => x.ID != action.ID).ToList();

        return state with
        {
            Cache = newCache
        };
    }

    [ReducerMethod]
    public static FeatureSchemaParameterDataParameterListState ReduceResult(FeatureSchemaParameterDataParameterListState state, DataParameterListResultAction action)
    {
        return state with
        {
            IsLoading = false,
            Cache = new(state.Cache) { [action.SchemaIdentifier] = action.Result.Select(x => x.GetValue()).ToList() },
            IsLoadingPerKey = new(state.IsLoadingPerKey) { [action.SchemaIdentifier] = false }
        };
    }

    [ReducerMethod]
    public static FeatureSchemaParameterDataParameterListState ReduceFailed(FeatureSchemaParameterDataParameterListState state, DataParameterListFailedAction action)
    {
        return state with
        {
            IsLoading = false,
            IsLoadingPerKey = new(state.IsLoadingPerKey) { [action.SchemaIdentifier] = false },
            ErrorMessage = action.Error
        };
    }
}

public class FeatureSchemaParameterDataParameterListEffects(APIHttpClient client)
{
    [EffectMethod]
    public async Task Handle(FeatureSchemaParameterDataParameterListReducers.LoadDataParameterListAction action, IDispatcher dispatcher)
    {
        try
        {
            RequestDataParameterList request = new() { SchemaIdentifier = action.SchemaIdentifier };
            ResponseDataParameterList result = await client.SendRequest(request);

            dispatcher.Dispatch(new FeatureSchemaParameterDataParameterListReducers.DataParameterListResultAction(action.SchemaIdentifier, result.Value));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new FeatureSchemaParameterDataParameterListReducers.DataParameterListFailedAction(action.SchemaIdentifier, ex.Message));
        }
    }
}
