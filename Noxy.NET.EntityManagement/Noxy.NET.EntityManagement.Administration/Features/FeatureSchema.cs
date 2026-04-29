using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public readonly record struct FeatureSchemaKey(string Scope, FeatureSchemaActionKind Kind);

public enum FeatureSchemaActionKind
{
    Find,
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
    public Dictionary<string, EntitySchema> Find { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchema>> List { get; init; } = [];
    public Dictionary<string, RequestSchemaList> Request { get; init; } = [];

    public bool TryGetLoading(string scope, FeatureSchemaActionKind kind, out bool value) => Loading.TryGetValue(new(scope, kind), out value);
    public bool TryGetError(string scope, FeatureSchemaActionKind kind, out string? error) => Error.TryGetValue(new(scope, kind), out error);

    public bool TryGetFind(string scope, [NotNullWhen(true)] out EntitySchema? value) => Find.TryGetValue(scope, out value);
    public bool TryGetList(string scope, [NotNullWhen(true)] out IReadOnlyList<EntitySchema>? list) => List.TryGetValue(scope, out list);
    public bool TryGetRequest(string scope, [NotNullWhen(true)] out RequestSchemaList? request) => Request.TryGetValue(scope, out request);
}

public static class FeatureSchemaReducers
{
    [ReducerMethod]
    public static FeatureSchemaState ReduceFind(FeatureSchemaState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureSchemaActionKind.Find);

    [ReducerMethod]
    public static FeatureSchemaState ReduceFindSuccess(FeatureSchemaState state, SuccessAction<EntitySchema> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Find = Set(next.Find, action.Scope, value) });

    [ReducerMethod]
    public static FeatureSchemaState ReduceFindFailure(FeatureSchemaState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceList(FeatureSchemaState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureSchemaActionKind.List, (next) => next with { Request = Set(next.Request, action.Scope, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaState ReduceListSuccess(FeatureSchemaState state, SuccessAction<List<EntitySchema>> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { List = Set(next.List, action.Scope, value) });

    [ReducerMethod]
    public static FeatureSchemaState ReduceListFailure(FeatureSchemaState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceAdd(FeatureSchemaState state, AddAction action) => StartAction(state, action.Scope, FeatureSchemaActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaState ReduceAddSuccess(FeatureSchemaState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceAddFailure(FeatureSchemaState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceUpdate(FeatureSchemaState state, UpdateAction action) => StartAction(state, action.Scope, FeatureSchemaActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaState ReduceUpdateSuccess(FeatureSchemaState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceUpdateFailure(FeatureSchemaState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceClone(FeatureSchemaState state, CloneAction action) => StartAction(state, action.Scope, FeatureSchemaActionKind.Clone);

    [ReducerMethod]
    public static FeatureSchemaState ReduceCloneSuccess(FeatureSchemaState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceCloneFailure(FeatureSchemaState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceDelete(FeatureSchemaState state, DeleteAction action) => StartAction(state, action.Scope, FeatureSchemaActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaState ReduceDeleteSuccess(FeatureSchemaState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceDeleteFailure(FeatureSchemaState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceActivate(FeatureSchemaState state, ActivateAction action) => StartAction(state, action.Scope, FeatureSchemaActionKind.Activate);

    [ReducerMethod]
    public static FeatureSchemaState ReduceActivateSuccess(FeatureSchemaState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceActivateFailure(FeatureSchemaState state, FailureAction action) => HandleFailureAction(state, action);

    private static Dictionary<TKey, TValue> Set<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key, TValue value) where TKey : notnull
    {
        return source.TryGetValue(key, out TValue? existing) && EqualityComparer<TValue>.Default.Equals(existing, value) ? source : new(source) { [key] = value };
    }

    private static FeatureSchemaState StartAction(FeatureSchemaState state, string scope, FeatureSchemaActionKind kind, Func<FeatureSchemaState, FeatureSchemaState>? cb = null)
    {
        FeatureSchemaKey key = new(scope, kind);
        FeatureSchemaState newState = state with
        {
            Loading = Set(state.Loading, key, true),
            Error = Set(state.Error, key, null)
        };

        return cb != null ? cb(newState) : newState;
    }

    private static FeatureSchemaState HandleSuccessAction<T>(FeatureSchemaState state, SuccessAction<T> action, Func<FeatureSchemaState, T, FeatureSchemaState>? cb = null)
    {
        FeatureSchemaKey key = new(action.Scope, action.Kind);
        FeatureSchemaState newState = state with
        {
            Loading = Set(state.Loading, key, false)
        };

        return cb != null ? cb(newState, action.Value) : newState;
    }

    private static FeatureSchemaState HandleFailureAction(FeatureSchemaState state, FailureAction action, Func<FeatureSchemaState, FailureAction, FeatureSchemaState>? cb = null)
    {
        FeatureSchemaKey key = new(action.Scope, action.Kind);
        FeatureSchemaState newState = state with
        {
            Loading = Set(state.Loading, key, false),
            Error = Set(state.Error, key, action.Error)
        };

        return cb != null ? cb(newState, action) : newState;
    }

    public record FindAction(string Scope, RequestSchemaFind Request);

    public record ListAction(string Scope, RequestSchemaList Request);

    public record AddAction(string Scope, RequestSchemaCreate Request);

    public record UpdateAction(string Scope, RequestSchemaUpdate Request);

    public record CloneAction(string Scope, RequestSchemaClone Request);

    public record DeleteAction(string Scope, RequestSchemaDelete Request);

    public record ActivateAction(string Scope, RequestSchemaActivate Request);

    public record SuccessAction<T>(string Scope, FeatureSchemaActionKind Kind, T Value);

    public record FailureAction(string Scope, FeatureSchemaActionKind Kind, string Error);
}

public class FeatureSchemaListEffects(APIHttpClient client, IState<FeatureSchemaState> state)
{
    [EffectMethod]
    public Task Handle(FindAction action, IDispatcher dispatcher)
    {
        return Execute(action.Scope, FeatureSchemaActionKind.Find, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(ListAction action, IDispatcher dispatcher)
    {
        return Execute(action.Scope, FeatureSchemaActionKind.List, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(AddAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(UpdateAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaActionKind.Update, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(CloneAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaActionKind.Clone, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(DeleteAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaActionKind.Delete, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(ActivateAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaActionKind.Activate, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    private static async Task Execute<TResult>(string scope, FeatureSchemaActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(scope, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SuccessAction<TResult>(scope, kind, result!));
        }
    }

    private async Task ExecuteWithRefresh<TResult>(string scope, FeatureSchemaActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(scope, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SuccessAction<TResult>(scope, kind, result!));
            RefreshList(scope, dispatcher);
        }
    }

    private static async Task<(bool Success, TResult? Result)> TryExecute<TResult>(string scope, FeatureSchemaActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
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
        if (state.Value.TryGetRequest(scope, out RequestSchemaList? request))
        {
            dispatcher.Dispatch(new ListAction(scope, request));
        }
    }
}
