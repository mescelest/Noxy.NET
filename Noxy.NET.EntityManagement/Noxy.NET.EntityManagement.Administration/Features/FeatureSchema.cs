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
    Update,
    Delete,
    Activate
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
        return state with { Loading = Set(state.Loading, key, true), Error = Set(state.Error, key, null), Request = Set(state.Request, action.Context, action.Request) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceListResult(FeatureSchemaState state, SchemaListResultAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.List);
        return state with { Loading = Set(state.Loading, key, false), Cache = Set(state.Cache, action.Context, action.Result) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceListFailed(FeatureSchemaState state, SchemaListFailedAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.List);
        return state with { Loading = Set(state.Loading, key, false), Error = Set(state.Error, key, action.Error) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceAdd(FeatureSchemaState state, SchemaAddAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Add);
        return state with { Loading = Set(state.Loading, key, false), Error = Set(state.Error, key, null) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceAddResult(FeatureSchemaState state, SchemaAddResultAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Add);
        return state with { Loading = Set(state.Loading, key, false) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceAddFailed(FeatureSchemaState state, SchemaAddFailedAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Add);
        return state with { Loading = Set(state.Loading, key, false), Error = Set(state.Error, key, action.Error) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceDelete(FeatureSchemaState state, SchemaDeleteAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Delete);
        return state with { Loading = Set(state.Loading, key, false), Error = Set(state.Error, key, null) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceDeleteResult(FeatureSchemaState state, SchemaDeleteResultAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Delete);
        return state with { Loading = Set(state.Loading, key, false) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceDeleteFailed(FeatureSchemaState state, SchemaDeleteFailedAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Delete);
        return state with { Loading = Set(state.Loading, key, false), Error = Set(state.Error, key, action.Error) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceActivate(FeatureSchemaState state, SchemaActivateAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Activate);
        return state with { Loading = Set(state.Loading, key, false), Error = Set(state.Error, key, null) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceActivateResult(FeatureSchemaState state, SchemaActivateResultAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Activate);
        return state with { Loading = Set(state.Loading, key, false) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceActivateFailed(FeatureSchemaState state, SchemaActivateFailedAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Activate);
        return state with { Loading = Set(state.Loading, key, false), Error = Set(state.Error, key, action.Error) };
    }

    private static Dictionary<TKey, TValue> Set<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key, TValue value) where TKey : notnull
    {
        return source.TryGetValue(key, out TValue? existing) && EqualityComparer<TValue>.Default.Equals(existing, value)
            ? source
            : new(source) { [key] = value };
    }


    public record SchemaListAction(string Context, RequestSchemaList Request);

    public record SchemaListResultAction(string Context, IReadOnlyList<EntitySchema> Result);

    public record SchemaListFailedAction(string Context, string Error);

    public record SchemaAddAction(string Context, RequestSchemaCreate Request);

    public record SchemaAddResultAction(string Context);

    public record SchemaAddFailedAction(string Context, string Error);

    public record SchemaDeleteAction(string Context, RequestSchemaDelete Request);

    public record SchemaDeleteResultAction(string Context);

    public record SchemaDeleteFailedAction(string Context, string Error);

    public record SchemaActivateAction(string Context, RequestSchemaActivate Request);

    public record SchemaActivateResultAction(string Context);

    public record SchemaActivateFailedAction(string Context, string Error);
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
            await client.SendRequest(action.Request);
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

    [EffectMethod]
    public async Task Handle(FeatureSchemaReducers.SchemaDeleteAction action, IDispatcher dispatcher)
    {
        try
        {
            await client.SendRequest(action.Request);
            dispatcher.Dispatch(new FeatureSchemaReducers.SchemaDeleteResultAction(action.Context));
            if (state.Value.TryGetRequest(action.Context, out RequestSchemaList? request))
            {
                dispatcher.Dispatch(new FeatureSchemaReducers.SchemaListAction(action.Context, request));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new FeatureSchemaReducers.SchemaDeleteFailedAction(action.Context, ex.Message));
        }
    }

    [EffectMethod]
    public async Task Handle(FeatureSchemaReducers.SchemaActivateAction action, IDispatcher dispatcher)
    {
        try
        {
            await client.SendRequest(action.Request);
            dispatcher.Dispatch(new FeatureSchemaReducers.SchemaActivateResultAction(action.Context));
            if (state.Value.TryGetRequest(action.Context, out RequestSchemaList? request))
            {
                dispatcher.Dispatch(new FeatureSchemaReducers.SchemaListAction(action.Context, request));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new FeatureSchemaReducers.SchemaActivateFailedAction(action.Context, ex.Message));
        }
    }
}
