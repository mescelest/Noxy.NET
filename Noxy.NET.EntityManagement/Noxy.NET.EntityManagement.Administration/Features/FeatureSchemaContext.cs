using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaContextReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public readonly record struct FeatureSchemaContextKey(string Scope, FeatureSchemaContextActionKind Kind);

public enum FeatureSchemaContextActionKind
{
    List,
    Add,
    Update,
    Delete,
    Clone
}

[FeatureState]
public record FeatureSchemaContextState
{
    public Dictionary<FeatureSchemaContextKey, bool> Loading { get; init; } = [];
    public Dictionary<FeatureSchemaContextKey, string?> Error { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchemaContext>> Cache { get; init; } = [];
    public Dictionary<string, RequestSchemaContextList> Request { get; init; } = [];

    public bool TryGetLoading(string scope, FeatureSchemaContextActionKind kind, out bool value) => Loading.TryGetValue(new(scope, kind), out value);
    public bool TryGetError(string scope, FeatureSchemaContextActionKind kind, out string? error) => Error.TryGetValue(new(scope, kind), out error);

    public bool TryGetList(string scope, [NotNullWhen(true)] out IReadOnlyList<EntitySchemaContext>? list) => Cache.TryGetValue(scope, out list);
    public bool TryGetRequest(string scope, [NotNullWhen(true)] out RequestSchemaContextList? request) => Request.TryGetValue(scope, out request);
}

public static class FeatureSchemaContextReducers
{
    [ReducerMethod]
    public static FeatureSchemaContextState ReduceList(FeatureSchemaContextState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureSchemaContextActionKind.List, (next) => next with { Request = Set(next.Request, action.Scope, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceListSuccess(FeatureSchemaContextState state, SuccessAction<List<EntitySchemaContext>> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Cache = Set(next.Cache, action.Scope, value) });

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceListFailure(FeatureSchemaContextState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceAdd(FeatureSchemaContextState state, AddAction action) => StartAction(state, action.Scope, FeatureSchemaContextActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceAddSuccess(FeatureSchemaContextState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceAddFailure(FeatureSchemaContextState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceUpdate(FeatureSchemaContextState state, UpdateAction action) => StartAction(state, action.Scope, FeatureSchemaContextActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceUpdateSuccess(FeatureSchemaContextState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceUpdateFailure(FeatureSchemaContextState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceClone(FeatureSchemaContextState state, CloneAction action) => StartAction(state, action.Scope, FeatureSchemaContextActionKind.Clone);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceCloneSuccess(FeatureSchemaContextState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceCloneFailure(FeatureSchemaContextState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceDelete(FeatureSchemaContextState state, DeleteAction action) => StartAction(state, action.Scope, FeatureSchemaContextActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceDeleteSuccess(FeatureSchemaContextState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceDeleteFailure(FeatureSchemaContextState state, FailureAction action) => HandleFailureAction(state, action);

    private static Dictionary<TKey, TValue> Set<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key, TValue value) where TKey : notnull
    {
        return source.TryGetValue(key, out TValue? existing) && EqualityComparer<TValue>.Default.Equals(existing, value) ? source : new(source) { [key] = value };
    }

    private static FeatureSchemaContextState StartAction(FeatureSchemaContextState state, string scope, FeatureSchemaContextActionKind kind, Func<FeatureSchemaContextState, FeatureSchemaContextState>? cb = null)
    {
        FeatureSchemaContextKey key = new(scope, kind);
        FeatureSchemaContextState newState = state with
        {
            Loading = Set(state.Loading, key, true),
            Error = Set(state.Error, key, null)
        };

        return cb != null ? cb(newState) : newState;
    }

    private static FeatureSchemaContextState HandleSuccessAction<T>(FeatureSchemaContextState state, SuccessAction<T> action, Func<FeatureSchemaContextState, T, FeatureSchemaContextState>? cb = null)
    {
        FeatureSchemaContextKey key = new(action.Scope, action.Kind);
        FeatureSchemaContextState newState = state with
        {
            Loading = Set(state.Loading, key, false)
        };

        return cb != null ? cb(newState, action.Value) : newState;
    }

    private static FeatureSchemaContextState HandleFailureAction(FeatureSchemaContextState state, FailureAction action, Func<FeatureSchemaContextState, FailureAction, FeatureSchemaContextState>? cb = null)
    {
        FeatureSchemaContextKey key = new(action.Scope, action.Kind);
        FeatureSchemaContextState newState = state with
        {
            Loading = Set(state.Loading, key, false),
            Error = Set(state.Error, key, action.Error)
        };

        return cb != null ? cb(newState, action) : newState;
    }

    public record ListAction(string Scope, RequestSchemaContextList Request);

    public record AddAction(string Scope, RequestSchemaContextCreate Request);

    public record UpdateAction(string Scope, RequestSchemaContextUpdate Request);

    public record CloneAction(string Scope, RequestSchemaContextClone Request);

    public record DeleteAction(string Scope, RequestSchemaContextDelete Request);

    public record SuccessAction<T>(string Scope, FeatureSchemaContextActionKind Kind, T Value);

    public record FailureAction(string Scope, FeatureSchemaContextActionKind Kind, string Error);
}

public class FeatureSchemaContextListEffects(APIHttpClient client, IState<FeatureSchemaContextState> state)
{
    [EffectMethod]
    public Task Handle(ListAction action, IDispatcher dispatcher)
    {
        return Execute(action.Scope, FeatureSchemaContextActionKind.List, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(AddAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaContextActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(UpdateAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaContextActionKind.Update, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(CloneAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaContextActionKind.Clone, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(DeleteAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaContextActionKind.Delete, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    private static async Task Execute<TResult>(string scope, FeatureSchemaContextActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(scope, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SuccessAction<TResult>(scope, kind, result!));
        }
    }

    private async Task ExecuteWithRefresh<TResult>(string scope, FeatureSchemaContextActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(scope, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SuccessAction<TResult>(scope, kind, result!));
            RefreshList(scope, dispatcher);
        }
    }

    private static async Task<(bool Success, TResult? Result)> TryExecute<TResult>(string scope, FeatureSchemaContextActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        try
        {
            TResult result = await operation();
            return (true, result);
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new FailureAction(scope, kind, ex.Message));
            return (false, default);
        }
    }

    private void RefreshList(string scope, IDispatcher dispatcher)
    {
        if (state.Value.TryGetRequest(scope, out RequestSchemaContextList? request))
        {
            dispatcher.Dispatch(new ListAction(scope, request));
        }
    }
}
