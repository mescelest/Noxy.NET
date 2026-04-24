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

    public bool TryGetLoading(string context, FeatureSchemaActionKind kind, out bool value) => Loading.TryGetValue(new(context, kind), out value);
    public bool TryGetError(string context, FeatureSchemaActionKind kind, out string? error) => Error.TryGetValue(new(context, kind), out error);

    public bool TryGetList(string context, [NotNullWhen(true)] out IReadOnlyList<EntitySchema>? list) => Cache.TryGetValue(context, out list);
    public bool TryGetRequest(string context, [NotNullWhen(true)] out RequestSchemaList? request) => Request.TryGetValue(context, out request);
}

public static class FeatureSchemaReducers
{
    [ReducerMethod]
    public static FeatureSchemaState ReduceList(FeatureSchemaState state, SchemaListAction action) =>
        StartAction(state, action.Context, FeatureSchemaActionKind.List, (next) => next with { Request = Set(next.Request, action.Context, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaState ReduceListResult(FeatureSchemaState state, SchemaResultAction<List<EntitySchema>> action) =>
        SuccessAction(state, action, (next, value) => next with { Cache = Set(next.Cache, action.Context, value) });

    [ReducerMethod]
    public static FeatureSchemaState ReduceListFailed(FeatureSchemaState state, SchemaFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceAdd(FeatureSchemaState state, SchemaAddAction action) => StartAction(state, action.Context, FeatureSchemaActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaState ReduceAddResult(FeatureSchemaState state, SchemaResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceAddFailed(FeatureSchemaState state, SchemaFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceUpdate(FeatureSchemaState state, SchemaUpdateAction action) => StartAction(state, action.Context, FeatureSchemaActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaState ReduceUpdateResult(FeatureSchemaState state, SchemaResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceUpdateFailed(FeatureSchemaState state, SchemaFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceClone(FeatureSchemaState state, SchemaCloneAction action) => StartAction(state, action.Context, FeatureSchemaActionKind.Clone);

    [ReducerMethod]
    public static FeatureSchemaState ReduceCloneResult(FeatureSchemaState state, SchemaResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceCloneFailed(FeatureSchemaState state, SchemaFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceDelete(FeatureSchemaState state, SchemaDeleteAction action) => StartAction(state, action.Context, FeatureSchemaActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaState ReduceDeleteResult(FeatureSchemaState state, SchemaResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceDeleteFailed(FeatureSchemaState state, SchemaFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceActivate(FeatureSchemaState state, SchemaActivateAction action) => StartAction(state, action.Context, FeatureSchemaActionKind.Activate);

    [ReducerMethod]
    public static FeatureSchemaState ReduceActivateResult(FeatureSchemaState state, SchemaResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceActivateFailed(FeatureSchemaState state, SchemaFailedAction action) => FailedAction(state, action);

    private static Dictionary<TKey, TValue> Set<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key, TValue value) where TKey : notnull
    {
        return source.TryGetValue(key, out TValue? existing) && EqualityComparer<TValue>.Default.Equals(existing, value)
            ? source
            : new(source) { [key] = value };
    }

    private static FeatureSchemaState StartAction(FeatureSchemaState state, string context, FeatureSchemaActionKind kind, Func<FeatureSchemaState, FeatureSchemaState>? cb = null)
    {
        FeatureSchemaKey key = new(context, kind);
        FeatureSchemaState newState = state with
        {
            Loading = Set(state.Loading, key, true),
            Error = Set(state.Error, key, null)
        };

        return cb != null ? cb(newState) : newState;
    }

    private static FeatureSchemaState SuccessAction<T>(FeatureSchemaState state, SchemaResultAction<T> action, Func<FeatureSchemaState, T, FeatureSchemaState>? cb = null)
    {
        FeatureSchemaKey key = new(action.Context, action.Kind);
        FeatureSchemaState newState = state with
        {
            Loading = Set(state.Loading, key, false)
        };

        return cb != null ? cb(newState, action.Value) : newState;
    }

    private static FeatureSchemaState FailedAction(FeatureSchemaState state, SchemaFailedAction action, Func<FeatureSchemaState, SchemaFailedAction, FeatureSchemaState>? cb = null)
    {
        FeatureSchemaKey key = new(action.Context, action.Kind);
        FeatureSchemaState newState = state with
        {
            Loading = Set(state.Loading, key, false),
            Error = Set(state.Error, key, action.Error)
        };

        return cb != null ? cb(newState, action) : newState;
    }

    public record SchemaListAction(string Context, RequestSchemaList Request);

    public record SchemaAddAction(string Context, RequestSchemaCreate Request);

    public record SchemaUpdateAction(string Context, RequestSchemaUpdate Request);

    public record SchemaCloneAction(string Context, RequestSchemaClone Request);

    public record SchemaDeleteAction(string Context, RequestSchemaDelete Request);

    public record SchemaActivateAction(string Context, RequestSchemaActivate Request);

    public record SchemaResultAction<T>(string Context, FeatureSchemaActionKind Kind, T Value);

    public record SchemaFailedAction(string Context, FeatureSchemaActionKind Kind, string Error);
}

public class FeatureSchemaListEffects(APIHttpClient client, IState<FeatureSchemaState> state)
{
    [EffectMethod]
    public Task Handle(SchemaListAction action, IDispatcher dispatcher)
    {
        return Execute(action.Context, FeatureSchemaActionKind.List, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaAddAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaUpdateAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaActionKind.Update, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaCloneAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaActionKind.Clone, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaDeleteAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaActionKind.Delete, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaActivateAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaActionKind.Activate, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    private static async Task Execute<TResult>(string context, FeatureSchemaActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(context, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SchemaResultAction<TResult>(context, kind, result!));
        }
    }

    private async Task ExecuteWithRefresh<TResult>(string context, FeatureSchemaActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(context, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SchemaResultAction<TResult>(context, kind, result!));
            RefreshList(context, dispatcher);
        }
    }

    private static async Task<(bool Success, TResult? Result)> TryExecute<TResult>(string context, FeatureSchemaActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        try
        {
            TResult result = await operation();
            return (true, result);
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new SchemaFailedAction(context, kind, ex.Message));
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
