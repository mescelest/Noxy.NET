using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public readonly record struct FeatureSchemaKey(string Context, FeatureSchemaActionKind Kind);

public enum FeatureSchemaActionKind
{
    List,
    Add,
    Update,
    Delete,
    Activate,
    Clone
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
    public static FeatureSchemaState ReduceListResult(FeatureSchemaState state, SchemaResultAction<List<EntitySchema>> action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.List);
        return state with { Loading = Set(state.Loading, key, false), Cache = Set(state.Cache, action.Context, action.Value) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceListFailed(FeatureSchemaState state, SchemaFailedAction action)
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
    public static FeatureSchemaState ReduceAddResult(FeatureSchemaState state, SchemaResultAction<Guid> action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Add);
        return state with { Loading = Set(state.Loading, key, false) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceAddFailed(FeatureSchemaState state, SchemaFailedAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Add);
        return state with { Loading = Set(state.Loading, key, false), Error = Set(state.Error, key, action.Error) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceClone(FeatureSchemaState state, SchemaCloneAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Clone);
        return state with { Loading = Set(state.Loading, key, false), Error = Set(state.Error, key, null) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceCloneResult(FeatureSchemaState state, SchemaResultAction<Guid> action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Clone);
        return state with { Loading = Set(state.Loading, key, false) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceCloneFailed(FeatureSchemaState state, SchemaFailedAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Clone);
        return state with { Loading = Set(state.Loading, key, false), Error = Set(state.Error, key, action.Error) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceDelete(FeatureSchemaState state, SchemaDeleteAction action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Delete);
        return state with { Loading = Set(state.Loading, key, false), Error = Set(state.Error, key, null) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceDeleteResult(FeatureSchemaState state, SchemaResultAction<Guid> action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Delete);
        return state with { Loading = Set(state.Loading, key, false) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceDeleteFailed(FeatureSchemaState state, SchemaFailedAction action)
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
    public static FeatureSchemaState ReduceActivateResult(FeatureSchemaState state, SchemaResultAction<Guid> action)
    {
        FeatureSchemaKey key = new(action.Context, FeatureSchemaActionKind.Activate);
        return state with { Loading = Set(state.Loading, key, false) };
    }

    [ReducerMethod]
    public static FeatureSchemaState ReduceActivateFailed(FeatureSchemaState state, SchemaFailedAction action)
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

    public record SchemaAddAction(string Context, RequestSchemaCreate Request);

    public record SchemaCloneAction(string Context, RequestSchemaClone Request);

    public record SchemaDeleteAction(string Context, RequestSchemaDelete Request);

    public record SchemaActivateAction(string Context, RequestSchemaActivate Request);

    public record SchemaResultAction<T>(string Context, T Value);

    public record SchemaFailedAction(string Context, string Error);
}

public class FeatureSchemaListEffects(APIHttpClient client, IState<FeatureSchemaState> state)
{
    [EffectMethod]
    public Task Handle(SchemaListAction action, IDispatcher dispatcher)
    {
        return Execute(action.Context, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaAddAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaCloneAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaDeleteAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaActivateAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    private static async Task Execute<TResult>(string context, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        var (success, result) = await TryExecute(operation, context, dispatcher);
        if (success)
        {
            dispatcher.Dispatch(new SchemaResultAction<TResult>(context, result!));
        }
    }


    private async Task ExecuteWithRefresh<TResult>(string context, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        var (success, result) = await TryExecute(operation, context, dispatcher);
        if (success)
        {
            dispatcher.Dispatch(new SchemaResultAction<TResult>(context, result!));
            RefreshList(context, dispatcher);
        }
    }

    private static async Task<(bool Success, TResult? Result)> TryExecute<TResult>(Func<Task<TResult>> operation, string context, IDispatcher dispatcher)
    {
        try
        {
            TResult result = await operation();
            return (true, result);
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new SchemaFailedAction(context, ex.Message));
            return (false, default);
        }
    }

    private void RefreshList(string context, IDispatcher dispatcher)
    {
        if (state.Value.TryGetRequest(context, out RequestSchemaList? request))
        {
            dispatcher.Dispatch(new SchemaListAction(context, request));
        }
    }
}
