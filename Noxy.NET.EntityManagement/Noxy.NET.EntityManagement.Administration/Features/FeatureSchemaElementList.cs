using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Administration.Features;

[FeatureState]
public record FeatureSchemaElementListState
{
    public bool IsLoading { get; init; }
    public Dictionary<Guid, bool> IsLoadingPerKey { get; init; } = new();
    public Dictionary<Guid, IReadOnlyList<EntitySchemaElement>> Cache { get; init; } = new();
    public string? ErrorMessage { get; init; }

    public bool IsKeyLoading(Guid schemaID) => IsLoadingPerKey.TryGetValue(schemaID, out bool isLoading) && isLoading;
}

public static class FeatureSchemaElementListReducers
{
    public record LoadSchemaElementListAction(Guid SchemaID);

    public record SchemaElementListResultAction(Guid SchemaID, IReadOnlyList<EntitySchemaElement> Result);

    public record SchemaElementListFailedAction(Guid SchemaID, string Error);

    [ReducerMethod]
    public static FeatureSchemaElementListState ReduceLoad(FeatureSchemaElementListState state, LoadSchemaElementListAction action)
    {
        return state with
        {
            IsLoading = true,
            IsLoadingPerKey = new(state.IsLoadingPerKey) { [action.SchemaID] = true },
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static FeatureSchemaElementListState ReduceResult(FeatureSchemaElementListState state, SchemaElementListResultAction action)
    {
        return state with
        {
            IsLoading = false,
            Cache = new(state.Cache) { [action.SchemaID] = action.Result.OrderBy(x => x.Order).ToList() },
            IsLoadingPerKey = new(state.IsLoadingPerKey) { [action.SchemaID] = false }
        };
    }

    [ReducerMethod]
    public static FeatureSchemaElementListState ReduceFailed(FeatureSchemaElementListState state, SchemaElementListFailedAction action)
    {
        return state with
        {
            IsLoading = false,
            IsLoadingPerKey = new(state.IsLoadingPerKey) { [action.SchemaID] = false },
            ErrorMessage = action.Error
        };
    }
}

public class FeatureSchemaElementListEffects(APIHttpClient client)
{
    [EffectMethod]
    public async Task Handle(FeatureSchemaElementListReducers.LoadSchemaElementListAction action, IDispatcher dispatcher)
    {
        try
        {
            RequestSchemaElementList request = new() { SchemaID = action.SchemaID };
            ResponseSchemaElementList result = await client.SendRequest(request);

            dispatcher.Dispatch(new FeatureSchemaElementListReducers.SchemaElementListResultAction(action.SchemaID, result.Value));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new FeatureSchemaElementListReducers.SchemaElementListFailedAction(action.SchemaID, ex.Message));
        }
    }
}
