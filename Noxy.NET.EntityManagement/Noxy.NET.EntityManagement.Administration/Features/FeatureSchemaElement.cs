using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaElementReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public readonly record struct FeatureSchemaElementKey(string Context, FeatureSchemaElementActionKind Kind);

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

    public bool TryGetLoading(string context, FeatureSchemaElementActionKind kind, out bool value) => Loading.TryGetValue(new(context, kind), out value);
    public bool TryGetError(string context, FeatureSchemaElementActionKind kind, out string? error) => Error.TryGetValue(new(context, kind), out error);

    public bool TryGetList(string context, [NotNullWhen(true)] out IReadOnlyList<EntitySchemaElement>? list) => Cache.TryGetValue(context, out list);
    public bool TryGetRequest(string context, [NotNullWhen(true)] out RequestSchemaElementList? request) => Request.TryGetValue(context, out request);
}

public static class FeatureSchemaElementReducers
{
    [ReducerMethod]
    public static FeatureSchemaElementState ReduceList(FeatureSchemaElementState state, SchemaElementListAction action) =>
        StartAction(state, action.Context, FeatureSchemaElementActionKind.List, (next) => next with { Request = Set(next.Request, action.Context, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceListResult(FeatureSchemaElementState state, SchemaElementResultAction<List<EntitySchemaElement>> action) =>
        SuccessAction(state, action, (next, value) => next with { Cache = Set(next.Cache, action.Context, value) });

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceListFailed(FeatureSchemaElementState state, SchemaElementFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceAdd(FeatureSchemaElementState state, SchemaElementAddAction action) => StartAction(state, action.Context, FeatureSchemaElementActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceAddResult(FeatureSchemaElementState state, SchemaElementResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceAddFailed(FeatureSchemaElementState state, SchemaElementFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceUpdate(FeatureSchemaElementState state, SchemaElementUpdateAction action) => StartAction(state, action.Context, FeatureSchemaElementActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceUpdateResult(FeatureSchemaElementState state, SchemaElementResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceUpdateFailed(FeatureSchemaElementState state, SchemaElementFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceClone(FeatureSchemaElementState state, SchemaElementCloneAction action) => StartAction(state, action.Context, FeatureSchemaElementActionKind.Clone);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceCloneResult(FeatureSchemaElementState state, SchemaElementResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceCloneFailed(FeatureSchemaElementState state, SchemaElementFailedAction action) => FailedAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceDelete(FeatureSchemaElementState state, SchemaElementDeleteAction action) => StartAction(state, action.Context, FeatureSchemaElementActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceDeleteResult(FeatureSchemaElementState state, SchemaElementResultAction<Guid> action) => SuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceDeleteFailed(FeatureSchemaElementState state, SchemaElementFailedAction action) => FailedAction(state, action);

    private static Dictionary<TKey, TValue> Set<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key, TValue value) where TKey : notnull
    {
        return source.TryGetValue(key, out TValue? existing) && EqualityComparer<TValue>.Default.Equals(existing, value)
            ? source
            : new(source) { [key] = value };
    }

    private static FeatureSchemaElementState StartAction(FeatureSchemaElementState state, string context, FeatureSchemaElementActionKind kind, Func<FeatureSchemaElementState, FeatureSchemaElementState>? cb = null)
    {
        FeatureSchemaElementKey key = new(context, kind);
        FeatureSchemaElementState newState = state with
        {
            Loading = Set(state.Loading, key, true),
            Error = Set(state.Error, key, null)
        };

        return cb != null ? cb(newState) : newState;
    }

    private static FeatureSchemaElementState SuccessAction<T>(FeatureSchemaElementState state, SchemaElementResultAction<T> action, Func<FeatureSchemaElementState, T, FeatureSchemaElementState>? cb = null)
    {
        FeatureSchemaElementKey key = new(action.Context, action.Kind);
        FeatureSchemaElementState newState = state with
        {
            Loading = Set(state.Loading, key, false)
        };

        return cb != null ? cb(newState, action.Value) : newState;
    }

    private static FeatureSchemaElementState FailedAction(FeatureSchemaElementState state, SchemaElementFailedAction action, Func<FeatureSchemaElementState, SchemaElementFailedAction, FeatureSchemaElementState>? cb = null)
    {
        FeatureSchemaElementKey key = new(action.Context, action.Kind);
        FeatureSchemaElementState newState = state with
        {
            Loading = Set(state.Loading, key, false),
            Error = Set(state.Error, key, action.Error)
        };

        return cb != null ? cb(newState, action) : newState;
    }

    public record SchemaElementListAction(string Context, RequestSchemaElementList Request);

    public record SchemaElementAddAction(string Context, RequestSchemaElementCreate Request);

    public record SchemaElementUpdateAction(string Context, RequestSchemaElementUpdate Request);

    public record SchemaElementCloneAction(string Context, RequestSchemaElementClone Request);

    public record SchemaElementDeleteAction(string Context, RequestSchemaElementDelete Request);

    public record SchemaElementResultAction<T>(string Context, FeatureSchemaElementActionKind Kind, T Value);

    public record SchemaElementFailedAction(string Context, FeatureSchemaElementActionKind Kind, string Error);
}

public class FeatureSchemaElementListEffects(APIHttpClient client, IState<FeatureSchemaElementState> state)
{
    [EffectMethod]
    public Task Handle(SchemaElementListAction action, IDispatcher dispatcher)
    {
        return Execute(action.Context, FeatureSchemaElementActionKind.List, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaElementAddAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaElementActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaElementUpdateAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaElementActionKind.Update, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaElementCloneAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaElementActionKind.Clone, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(SchemaElementDeleteAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Context, FeatureSchemaElementActionKind.Delete, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    private static async Task Execute<TResult>(string context, FeatureSchemaElementActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(context, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SchemaElementResultAction<TResult>(context, kind, result!));
        }
    }

    private async Task ExecuteWithRefresh<TResult>(string context, FeatureSchemaElementActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(context, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SchemaElementResultAction<TResult>(context, kind, result!));
            RefreshList(context, dispatcher);
        }
    }

    private static async Task<(bool Success, TResult? Result)> TryExecute<TResult>(string context, FeatureSchemaElementActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        try
        {
            TResult result = await operation();
            return (true, result);
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new SchemaElementFailedAction(context, kind, ex.Message));
            return (false, default);
        }
    }

    private void RefreshList(string context, IDispatcher dispatcher)
    {
        if (state.Value.TryGetRequest(context, out RequestSchemaElementList? request))
        {
            dispatcher.Dispatch(new SchemaElementListAction(context, request));
        }
    }
}
