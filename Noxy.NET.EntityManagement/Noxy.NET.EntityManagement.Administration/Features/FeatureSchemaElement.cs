using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaElementReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public readonly record struct FeatureSchemaElementKey(string Scope, FeatureSchemaElementActionKind Kind);

public enum FeatureSchemaElementActionKind
{
    List,
    Add,
    Update,
    Delete,
    Clone
}

[FeatureState]
public record FeatureSchemaElementState
{
    public Dictionary<FeatureSchemaElementKey, bool> Loading { get; init; } = [];
    public Dictionary<FeatureSchemaElementKey, string?> Error { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchemaElement>> Cache { get; init; } = [];
    public Dictionary<string, RequestSchemaElementList> Request { get; init; } = [];

    public bool TryGetLoading(string scope, FeatureSchemaElementActionKind kind, out bool value) => Loading.TryGetValue(new(scope, kind), out value);
    public bool TryGetError(string scope, FeatureSchemaElementActionKind kind, out string? error) => Error.TryGetValue(new(scope, kind), out error);

    public bool TryGetList(string scope, [NotNullWhen(true)] out IReadOnlyList<EntitySchemaElement>? list) => Cache.TryGetValue(scope, out list);
    public bool TryGetRequest(string scope, [NotNullWhen(true)] out RequestSchemaElementList? request) => Request.TryGetValue(scope, out request);
}

public static class FeatureSchemaElementReducers
{
    [ReducerMethod]
    public static FeatureSchemaElementState ReduceList(FeatureSchemaElementState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureSchemaElementActionKind.List, (next) => next with { Request = Set(next.Request, action.Scope, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceListSuccess(FeatureSchemaElementState state, SuccessAction<List<EntitySchemaElement>> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Cache = Set(next.Cache, action.Scope, value) });

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceListFailure(FeatureSchemaElementState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceAdd(FeatureSchemaElementState state, AddAction action) => StartAction(state, action.Scope, FeatureSchemaElementActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceAddSuccess(FeatureSchemaElementState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceAddFailure(FeatureSchemaElementState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceUpdate(FeatureSchemaElementState state, UpdateAction action) => StartAction(state, action.Scope, FeatureSchemaElementActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceUpdateSuccess(FeatureSchemaElementState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceUpdateFailure(FeatureSchemaElementState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceClone(FeatureSchemaElementState state, CloneAction action) => StartAction(state, action.Scope, FeatureSchemaElementActionKind.Clone);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceCloneSuccess(FeatureSchemaElementState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceCloneFailure(FeatureSchemaElementState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceDelete(FeatureSchemaElementState state, DeleteAction action) => StartAction(state, action.Scope, FeatureSchemaElementActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceDeleteSuccess(FeatureSchemaElementState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceDeleteFailure(FeatureSchemaElementState state, FailureAction action) => HandleFailureAction(state, action);

    private static Dictionary<TKey, TValue> Set<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key, TValue value) where TKey : notnull
    {
        return source.TryGetValue(key, out TValue? existing) && EqualityComparer<TValue>.Default.Equals(existing, value) ? source : new(source) { [key] = value };
    }

    private static FeatureSchemaElementState StartAction(FeatureSchemaElementState state, string scope, FeatureSchemaElementActionKind kind, Func<FeatureSchemaElementState, FeatureSchemaElementState>? cb = null)
    {
        FeatureSchemaElementKey key = new(scope, kind);
        FeatureSchemaElementState newState = state with
        {
            Loading = Set(state.Loading, key, true),
            Error = Set(state.Error, key, null)
        };

        return cb != null ? cb(newState) : newState;
    }

    private static FeatureSchemaElementState HandleSuccessAction<T>(FeatureSchemaElementState state, SuccessAction<T> action, Func<FeatureSchemaElementState, T, FeatureSchemaElementState>? cb = null)
    {
        FeatureSchemaElementKey key = new(action.Scope, action.Kind);
        FeatureSchemaElementState newState = state with
        {
            Loading = Set(state.Loading, key, false)
        };

        return cb != null ? cb(newState, action.Value) : newState;
    }

    private static FeatureSchemaElementState HandleFailureAction(FeatureSchemaElementState state, FailureAction action, Func<FeatureSchemaElementState, FailureAction, FeatureSchemaElementState>? cb = null)
    {
        FeatureSchemaElementKey key = new(action.Scope, action.Kind);
        FeatureSchemaElementState newState = state with
        {
            Loading = Set(state.Loading, key, false),
            Error = Set(state.Error, key, action.Error)
        };

        return cb != null ? cb(newState, action) : newState;
    }

    public record ListAction(string Scope, RequestSchemaElementList Request);

    public record AddAction(string Scope, RequestSchemaElementCreate Request);

    public record UpdateAction(string Scope, RequestSchemaElementUpdate Request);

    public record CloneAction(string Scope, RequestSchemaElementClone Request);

    public record DeleteAction(string Scope, RequestSchemaElementDelete Request);

    public record SuccessAction<T>(string Scope, FeatureSchemaElementActionKind Kind, T Value);

    public record FailureAction(string Scope, FeatureSchemaElementActionKind Kind, string Error);
}

public class FeatureSchemaElementEffects(APIHttpClient client, IState<FeatureSchemaElementState> state)
{
    [EffectMethod]
    public Task Handle(ListAction action, IDispatcher dispatcher)
    {
        return Execute(action.Scope, FeatureSchemaElementActionKind.List, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(AddAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaElementActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(UpdateAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaElementActionKind.Update, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(CloneAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaElementActionKind.Clone, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(DeleteAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaElementActionKind.Delete, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    private static async Task Execute<TResult>(string scope, FeatureSchemaElementActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(scope, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SuccessAction<TResult>(scope, kind, result!));
        }
    }

    private async Task ExecuteWithRefresh<TResult>(string scope, FeatureSchemaElementActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(scope, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SuccessAction<TResult>(scope, kind, result!));
            RefreshList(scope, dispatcher);
        }
    }

    private static async Task<(bool Success, TResult? Result)> TryExecute<TResult>(string scope, FeatureSchemaElementActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
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
        if (state.Value.TryGetRequest(scope, out RequestSchemaElementList? request))
        {
            dispatcher.Dispatch(new ListAction(scope, request));
        }
    }
}
