using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Administration.Features;

public readonly record struct FeatureSchemaKey(string Context, FeatureSchemaActionKind Kind);

public enum FeatureSchemaActionKind
{
    List,
    Add,
    Delete,
    Update
}

[FeatureState]
public record FeatureSchemaState
{
    public Dictionary<FeatureSchemaKey, bool> Loading { get; init; } = [];
    public Dictionary<FeatureSchemaKey, string?> Error { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchema>> Cache { get; init; } = [];
    public Dictionary<string, RequestSchemaList> Request { get; init; } = [];

    public bool TryGetLoading(string context, FeatureSchemaActionKind kind, out bool list) => Loading.TryGetValue(new(context, kind), out list);
    public bool TryGetError(string context, FeatureSchemaActionKind kind, out string? list) => Error.TryGetValue(new(context, kind), out list);

    public bool TryGetList(string context, [NotNullWhen(true)] out IReadOnlyList<EntitySchema>? list) => Cache.TryGetValue(context, out list);
    public bool TryGetRequest(string context, [NotNullWhen(true)] out RequestSchemaList? list) => Request.TryGetValue(context, out list);
}

public static class FeatureSchemaReducers
{
    [ReducerMethod]
    public static FeatureSchemaState ReduceList(FeatureSchemaState state, SchemaListAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.List);

        return state with
        {
            Loading = new(state.Loading) { [key] = true },
            Request = new(state.Request) { [action.Context] = action.Request },
        };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceListResult(FeatureSchemaState state, SchemaListResultAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.List);

        return state with
        {
            Loading = new(state.Loading) { [key] = false },
            Cache = new(state.Cache) { [action.Context] = action.Result },
        };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceListFailed(FeatureSchemaState state, SchemaListFailedAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.List);

        return state with
        {
            Loading = new(state.Loading) { [key] = false },
            Error = new(state.Error) { [key] = action.Error },
        };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceAdd(FeatureSchemaState state, SchemaAddAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.List);

        return state with
        {
            Loading = new(state.Loading) { [key] = true },
        };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceAddResult(FeatureSchemaState state, SchemaAddResultAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.List);

        return state with
        {
            Loading = new(state.Loading) { [key] = false },
        };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceAddFailed(FeatureSchemaState state, SchemaAddFailedAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.List);

        return state with
        {
            Loading = new(state.Loading) { [key] = false },
            Error = new(state.Error) { [key] = action.Error },
        };
    }

    public record SchemaListAction(string Context, RequestSchemaList Request);

    public record SchemaListResultAction(string Context, IReadOnlyList<EntitySchema> Result);

    public record SchemaListFailedAction(string Context, string Error);

    public record SchemaAddAction(string Context, RequestSchemaCreate Request);

    public record SchemaAddResultAction(string Context);

    public record SchemaAddFailedAction(string Context, string Error);
}

public class FeatureSchemaListEffects(APIHttpClient client, IState<FeatureSchemaState> state)
{
    [EffectMethod]
    public async Task Handle(FeatureSchemaReducers.SchemaListAction action, IDispatcher dispatcher)
    {
        try
        {
            ResponseSchemaList result = await client.SendRequest(action.Request);
            dispatcher.Dispatch(new FeatureSchemaReducers.SchemaListResultAction(action.Context, result.Value));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new FeatureSchemaReducers.SchemaListFailedAction(action.Context, ex.Message));
        }
    }

    [EffectMethod]
    public async Task Handle(FeatureSchemaReducers.SchemaAddAction action, IDispatcher dispatcher)
    {
        try
        {
            ResponseSchemaCreate result = await client.SendRequest(action.Request);
            dispatcher.Dispatch(new FeatureSchemaReducers.SchemaAddResultAction(action.Context));
            if (state.Value.TryGetRequest(action.Context, out RequestSchemaList? request))
            {
                dispatcher.Dispatch(new FeatureSchemaReducers.SchemaListAction(action.Context, request));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new FeatureSchemaReducers.SchemaAddFailedAction(action.Context, ex.Message));
        }
    }
}
