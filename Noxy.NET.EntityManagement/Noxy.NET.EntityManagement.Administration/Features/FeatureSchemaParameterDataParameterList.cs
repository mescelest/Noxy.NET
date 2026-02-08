using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Administration.Features;

[FeatureState]
public record FeatureSchemaParameterDataParameterListState
{
    public bool IsLoading { get; init; }
    public Dictionary<string, IReadOnlyList<EntityDataParameter.Discriminator>> Cache { get; init; } = new();
    public string? ErrorMessage { get; init; }
}

public static class FeatureSchemaParameterDataParameterListReducers
{
    [ReducerMethod]
    public static FeatureSchemaParameterDataParameterListState ReduceLoad(FeatureSchemaParameterDataParameterListState state, LoadDataParameterListAction action)
    {
        return state with { IsLoading = true, ErrorMessage = null };
    }

    [ReducerMethod]
    public static FeatureSchemaParameterDataParameterListState ReduceResult(FeatureSchemaParameterDataParameterListState state, DataParameterListResultAction action)
    {
        Dictionary<string, IReadOnlyList<EntityDataParameter.Discriminator>> updated = new(state.Cache)
        {
            [action.SchemaIdentifier] = action.Results
        };

        return state with { IsLoading = false, Cache = updated };
    }

    [ReducerMethod]
    public static FeatureSchemaParameterDataParameterListState ReduceFailed(FeatureSchemaParameterDataParameterListState state, DataParameterListFailedAction action)
    {
        return state with { IsLoading = false, ErrorMessage = action.Error };
    }

    public record LoadDataParameterListAction(string SchemaIdentifier);

    public record DataParameterListResultAction(string SchemaIdentifier, IReadOnlyList<EntityDataParameter.Discriminator> Results);

    public record DataParameterListFailedAction(string Error);
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
            dispatcher.Dispatch(new FeatureSchemaParameterDataParameterListReducers.DataParameterListFailedAction(ex.Message));
        }
    }
}
