using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Administration.Features;

[FeatureState]
public record FeatureSchemaParameterListState
{
    public bool IsLoading { get; init; }
    public IReadOnlyList<EntitySchemaParameter.Discriminator> List { get; init; } = [];
    public string? ErrorMessage { get; init; }
}

public static class FeatureSchemaParameterListReducers
{
    [ReducerMethod]
    public static FeatureSchemaParameterListState ReduceLoad(FeatureSchemaParameterListState state, SchemaParameterListAction action)
    {
        return state with { IsLoading = true, ErrorMessage = null };
    }

    [ReducerMethod]
    public static FeatureSchemaParameterListState ReduceResult(FeatureSchemaParameterListState state, SchemaParameterListResultAction action)
    {
        return state with { IsLoading = false, List = action.Result };
    }

    [ReducerMethod]
    public static FeatureSchemaParameterListState ReduceFailed(FeatureSchemaParameterListState state, SchemaParameterListFailedAction action)
    {
        return state with { IsLoading = false, ErrorMessage = action.Error };
    }

    public record SchemaParameterListAction(RequestSchemaParameterList Request);

    public record SchemaParameterListResultAction(IReadOnlyList<EntitySchemaParameter.Discriminator> Result);

    public record SchemaParameterListFailedAction(string Error);
}

public class FeatureSchemaParameterListEffects(APIHttpClient client)
{
    [EffectMethod]
    public async Task Handle(FeatureSchemaParameterListReducers.SchemaParameterListAction action, IDispatcher dispatcher)
    {
        try
        {
            ResponseSchemaParameterList result = await client.SendRequest(action.Request);
            dispatcher.Dispatch(new FeatureSchemaParameterListReducers.SchemaParameterListResultAction(result.Value));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new FeatureSchemaParameterListReducers.SchemaParameterListFailedAction(ex.Message));
        }
    }
}
