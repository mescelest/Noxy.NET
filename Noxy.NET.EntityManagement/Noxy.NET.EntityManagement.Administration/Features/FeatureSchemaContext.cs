using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaContextReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public readonly record struct FeatureSchemaContextKey(string Context, FeatureSchemaContextActionKind Kind);

public enum FeatureSchemaContextActionKind
{
    List,
    Add,
    Update,
    Delete,
    Activate,
    Clone
}

[FeatureState]
public record FeatureSchemaContextState
{
    public Dictionary<FeatureSchemaContextKey, bool> Loading { get; init; } = [];
    public Dictionary<FeatureSchemaContextKey, string?> Error { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchemaContext>> Cache { get; init; } = [];
    public Dictionary<string, RequestSchemaContextList> Request { get; init; } = [];

    public bool TryGetLoading(string context, FeatureSchemaContextActionKind kind, out bool value) => Loading.TryGetValue(new(context, kind), out value);
    public bool TryGetError(string context, FeatureSchemaContextActionKind kind, out string? error) => Error.TryGetValue(new(context, kind), out error);

    public bool TryGetList(string context, [NotNullWhen(true)] out IReadOnlyList<EntitySchemaContext>? list) => Cache.TryGetValue(context, out list);
    public bool TryGetRequest(string context, [NotNullWhen(true)] out RequestSchemaContextList? request) => Request.TryGetValue(context, out request);
}

public static class FeatureSchemaContextReducers
{
    [ReducerMethod]
    public static FeatureSchemaContextState ReduceList(FeatureSchemaContextState state, SchemaContextListAction action) =>
        StartAction(state, action.Context, FeatureSchemaContextActionKind.List, (next) => next with { Request = Set(next.Request, action.Context, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceListResult(FeatureSchemaContextState state, SchemaContextResultAction<List<EntitySchemaContext>> action) =>
        SuccessAction(state, action, (next, value) => next with { Cache = Set(next.Cache, action.Context, value) });

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceListFailed(FeatureSchemaContextState state, SchemaContextFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceAdd(FeatureSchemaContextState state, SchemaContextAddAction action) => StartAction(state, action.Context, FeatureSchemaContextActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceAddResult(FeatureSchemaContextState state, SchemaContextResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceAddFailed(FeatureSchemaContextState state, SchemaContextFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceUpdate(FeatureSchemaContextState state, SchemaContextUpdateAction action) => StartAction(state, action.Context, FeatureSchemaContextActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceUpdateResult(FeatureSchemaContextState state, SchemaContextResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceUpdateFailed(FeatureSchemaContextState state, SchemaContextFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceClone(FeatureSchemaContextState state, SchemaContextCloneAction action) => StartAction(state, action.Context, FeatureSchemaContextActionKind.Clone);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceCloneResult(FeatureSchemaContextState state, SchemaContextResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceCloneFailed(FeatureSchemaContextState state, SchemaContextFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceDelete(FeatureSchemaContextState state, SchemaContextDeleteAction action) => StartAction(state, action.Context, FeatureSchemaContextActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceDeleteResult(FeatureSchemaContextState state, SchemaContextResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceDeleteFailed(FeatureSchemaContextState state, SchemaContextFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceActivate(FeatureSchemaContextState state, SchemaContextActivateAction action) => StartAction(state, action.Context, FeatureSchemaContextActionKind.Activate);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceActivateResult(FeatureSchemaContextState state, SchemaContextResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceActivateFailed(FeatureSchemaContextState state, SchemaContextFailedAction action) => FailedAction(state, action);

    private static Dictionary<TKey, TValue> Set<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key, TValue value) where TKey : notnull
    {
        return source.TryGetValue(key, out TValue? existing) && EqualityComparer<TValue>.Default.Equals(existing, value)
            ? source
            : new(source) { [key] = value };
    }

    private static FeatureSchemaContextState StartAction(FeatureSchemaContextState state, string context, FeatureSchemaContextActionKind kind, Func<FeatureSchemaContextState, FeatureSchemaContextState>? cb = null)
    {
        FeatureSchemaContextKey key = new(context, kind);
        FeatureSchemaContextState newState = state with
        {
            Loading = Set(state.Loading, key, true),
            Error = Set(state.Error, key, null)
        };

        return cb != null ? cb(newState) : newState;
    }

    private static FeatureSchemaContextState SuccessAction<T>(FeatureSchemaContextState state, SchemaContextResultAction<T> action, Func<FeatureSchemaContextState, T, FeatureSchemaContextState>? cb = null)
    {
        FeatureSchemaContextKey key = new(action.Context, action.Kind);
        FeatureSchemaContextState newState = state with
        {
            Loading = Set(state.Loading, key, false)
        };

        return cb != null ? cb(newState, action.Value) : newState;
    }

    private static FeatureSchemaContextState FailedAction(FeatureSchemaContextState state, SchemaContextFailedAction action, Func<FeatureSchemaContextState, SchemaContextFailedAction, FeatureSchemaContextState>? cb = null)
    {
        FeatureSchemaContextKey key = new(action.Context, action.Kind);
        FeatureSchemaContextState newState = state with
        {
            Loading = Set(state.Loading, key, false),
            Error = Set(state.Error, key, action.Error)
        };

        return cb != null ? cb(newState, action) : newState;
    }

    public record SchemaContextListAction(string Context, RequestSchemaContextList Request);

    public record SchemaContextAddAction(string Context, RequestSchemaCreate Request);

    public record SchemaContextUpdateAction(string Context, RequestSchemaUpdate Request);

    public record SchemaContextCloneAction(string Context, RequestSchemaClone Request);

    public record SchemaContextDeleteAction(string Context, RequestSchemaDelete Request);

    public record SchemaContextActivateAction(string Context, RequestSchemaActivate Request);

    public record SchemaContextResultAction<T>(string Context, FeatureSchemaContextActionKind Kind, T Value);

    public record SchemaContextFailedAction(string Context, FeatureSchemaContextActionKind Kind, string Error);
}

public class FeatureSchemaContextListEffects(APIHttpClient client, IState<FeatureSchemaContextState> state)
{
    [EffectMethod]
    public Task Handle(SchemaContextListAction action, IDispatcher dispatcher)
    {
        return Execute(action.Context, FeatureSchemaContextActionKind.List, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaContextAddAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaContextActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaContextUpdateAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaContextActionKind.Update, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaContextCloneAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaContextActionKind.Clone, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaContextDeleteAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaContextActionKind.Delete, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaContextActivateAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaContextActionKind.Activate, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    private static async Task Execute<TResult>(string context, FeatureSchemaContextActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(context, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SchemaContextResultAction<TResult>(context, kind, result!));
        }
    }

    private async Task ExecuteWithRefresh<TResult>(string context, FeatureSchemaContextActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(context, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SchemaContextResultAction<TResult>(context, kind, result!));
            RefreshList(context, dispatcher);
        }
    }

    private static async Task<(bool Success, TResult? Result)> TryExecute<TResult>(string context, FeatureSchemaContextActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        try
        {
            TResult result = await operation();
            return (true, result);
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new SchemaContextFailedAction(context, kind, ex.Message));
            return (false, default);
        }
    }

    private void RefreshList(string context, IDispatcher dispatcher)
    {
        if (state.Value.TryGetRequest(context, out RequestSchemaContextList? request))
        {
            dispatcher.Dispatch(new SchemaContextListAction(context, request));
        }
    }
}
