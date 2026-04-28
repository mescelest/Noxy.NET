using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaParameterReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public readonly record struct FeatureSchemaParameterKey(string Context, FeatureSchemaParameterActionKind Kind);

public enum FeatureSchemaParameterActionKind
{
    List,
    Add,
    Update,
    Delete,
    Clone
}

[FeatureState]
public record FeatureSchemaParameterState
{
    public Dictionary<FeatureSchemaParameterKey, bool> Loading { get; init; } = [];
    public Dictionary<FeatureSchemaParameterKey, string?> Error { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchemaParameter.Discriminator>> Cache { get; init; } = [];
    public Dictionary<string, RequestSchemaParameterList> Request { get; init; } = [];

    public bool TryGetLoading(string context, FeatureSchemaParameterActionKind kind, out bool value) => Loading.TryGetValue(new(context, kind), out value);
    public bool TryGetError(string context, FeatureSchemaParameterActionKind kind, out string? error) => Error.TryGetValue(new(context, kind), out error);

    public bool TryGetList(string context, [NotNullWhen(true)] out IReadOnlyList<EntitySchemaParameter.Discriminator>? list) => Cache.TryGetValue(context, out list);
    public bool TryGetRequest(string context, [NotNullWhen(true)] out RequestSchemaParameterList? request) => Request.TryGetValue(context, out request);
}

public static class FeatureSchemaParameterReducers
{
    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceList(FeatureSchemaParameterState state, SchemaParameterListAction action) =>
        StartAction(state, action.Context, FeatureSchemaParameterActionKind.List, (next) => next with { Request = Set(next.Request, action.Context, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceListResult(FeatureSchemaParameterState state, SchemaParameterResultAction<List<EntitySchemaParameter.Discriminator>> action) =>
        SuccessAction(state, action, (next, value) => next with { Cache = Set(next.Cache, action.Context, value) });

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceListFailed(FeatureSchemaParameterState state, SchemaParameterFailedAction action) => FailedAction(state, action);

    // [ReducerMethod]
    // public static FeatureSchemaParameterState ReduceAdd(FeatureSchemaParameterState state, SchemaParameterAddAction action) => StartAction(state, action.Context, FeatureSchemaParameterActionKind.Add);
    //
    // [ReducerMethod]
    // public static FeatureSchemaParameterState ReduceAddResult(FeatureSchemaParameterState state, SchemaParameterResultAction<Guid> action) => SuccessAction(state, action);
    //
    // [ReducerMethod]
    // public static FeatureSchemaParameterState ReduceAddFailed(FeatureSchemaParameterState state, SchemaParameterFailedAction action) => FailedAction(state, action);
    //
    // [ReducerMethod]
    // public static FeatureSchemaParameterState ReduceUpdate(FeatureSchemaParameterState state, SchemaParameterUpdateAction action) => StartAction(state, action.Context, FeatureSchemaParameterActionKind.Update);
    //
    // [ReducerMethod]
    // public static FeatureSchemaParameterState ReduceUpdateResult(FeatureSchemaParameterState state, SchemaParameterResultAction<Guid> action) => SuccessAction(state, action);
    //
    // [ReducerMethod]
    // public static FeatureSchemaParameterState ReduceUpdateFailed(FeatureSchemaParameterState state, SchemaParameterFailedAction action) => FailedAction(state, action);
    //
    // [ReducerMethod]
    // public static FeatureSchemaParameterState ReduceClone(FeatureSchemaParameterState state, SchemaParameterCloneAction action) => StartAction(state, action.Context, FeatureSchemaParameterActionKind.Clone);
    //
    // [ReducerMethod]
    // public static FeatureSchemaParameterState ReduceCloneResult(FeatureSchemaParameterState state, SchemaParameterResultAction<Guid> action) => SuccessAction(state, action);
    //
    // [ReducerMethod]
    // public static FeatureSchemaParameterState ReduceCloneFailed(FeatureSchemaParameterState state, SchemaParameterFailedAction action) => FailedAction(state, action);
    //
    // [ReducerMethod]
    // public static FeatureSchemaParameterState ReduceDelete(FeatureSchemaParameterState state, SchemaParameterDeleteAction action) => StartAction(state, action.Context, FeatureSchemaParameterActionKind.Delete);
    //
    // [ReducerMethod]
    // public static FeatureSchemaParameterState ReduceDeleteResult(FeatureSchemaParameterState state, SchemaParameterResultAction<Guid> action) => SuccessAction(state, action);
    //
    // [ReducerMethod]
    // public static FeatureSchemaParameterState ReduceDeleteFailed(FeatureSchemaParameterState state, SchemaParameterFailedAction action) => FailedAction(state, action);

    private static Dictionary<TKey, TValue> Set<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key, TValue value) where TKey : notnull
    {
        return source.TryGetValue(key, out TValue? existing) && EqualityComparer<TValue>.Default.Equals(existing, value)
            ? source
            : new(source) { [key] = value };
    }

    private static FeatureSchemaParameterState StartAction(FeatureSchemaParameterState state, string context, FeatureSchemaParameterActionKind kind, Func<FeatureSchemaParameterState, FeatureSchemaParameterState>? cb = null)
    {
        FeatureSchemaParameterKey key = new(context, kind);
        FeatureSchemaParameterState newState = state with
        {
            Loading = Set(state.Loading, key, true),
            Error = Set(state.Error, key, null)
        };

        return cb != null ? cb(newState) : newState;
    }

    private static FeatureSchemaParameterState SuccessAction<T>(FeatureSchemaParameterState state, SchemaParameterResultAction<T> action, Func<FeatureSchemaParameterState, T, FeatureSchemaParameterState>? cb = null)
    {
        FeatureSchemaParameterKey key = new(action.Context, action.Kind);
        FeatureSchemaParameterState newState = state with
        {
            Loading = Set(state.Loading, key, false)
        };

        return cb != null ? cb(newState, action.Value) : newState;
    }

    private static FeatureSchemaParameterState FailedAction(FeatureSchemaParameterState state, SchemaParameterFailedAction action, Func<FeatureSchemaParameterState, SchemaParameterFailedAction, FeatureSchemaParameterState>? cb = null)
    {
        FeatureSchemaParameterKey key = new(action.Context, action.Kind);
        FeatureSchemaParameterState newState = state with
        {
            Loading = Set(state.Loading, key, false),
            Error = Set(state.Error, key, action.Error)
        };

        return cb != null ? cb(newState, action) : newState;
    }

    public record SchemaParameterListAction(string Context, RequestSchemaParameterList Request);

    // public record SchemaParameterAddAction(string Context, RequestSchemaParameterCreate Request);
    //
    // public record SchemaParameterUpdateAction(string Context, RequestSchemaParameterUpdate Request);
    //
    // public record SchemaParameterCloneAction(string Context, RequestSchemaParameterClone Request);
    //
    // public record SchemaParameterDeleteAction(string Context, RequestSchemaParameterDelete Request);

    public record SchemaParameterResultAction<T>(string Context, FeatureSchemaParameterActionKind Kind, T Value);

    public record SchemaParameterFailedAction(string Context, FeatureSchemaParameterActionKind Kind, string Error);
}

public class FeatureSchemaParameterEffects(APIHttpClient client, IState<FeatureSchemaParameterState> state)
{
    [EffectMethod]
    public Task Handle(SchemaParameterListAction action, IDispatcher dispatcher)
    {
        return Execute(action.Context, FeatureSchemaParameterActionKind.List, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    // [EffectMethod]
    // public Task Handle(SchemaParameterAddAction action, IDispatcher dispatcher)
    // {
    //     return ExecuteWithRefresh(action.Context, FeatureSchemaParameterActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    // }
    //
    // [EffectMethod]
    // public Task Handle(SchemaParameterUpdateAction action, IDispatcher dispatcher)
    // {
    //     return ExecuteWithRefresh(action.Context, FeatureSchemaParameterActionKind.Update, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    // }
    //
    // [EffectMethod]
    // public Task Handle(SchemaParameterCloneAction action, IDispatcher dispatcher)
    // {
    //     return ExecuteWithRefresh(action.Context, FeatureSchemaParameterActionKind.Clone, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    // }
    //
    // [EffectMethod]
    // public Task Handle(SchemaParameterDeleteAction action, IDispatcher dispatcher)
    // {
    //     return ExecuteWithRefresh(action.Context, FeatureSchemaParameterActionKind.Delete, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    // }

    private static async Task Execute<TResult>(string context, FeatureSchemaParameterActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(context, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SchemaParameterResultAction<TResult>(context, kind, result!));
        }
    }

    private async Task ExecuteWithRefresh<TResult>(string context, FeatureSchemaParameterActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(context, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SchemaParameterResultAction<TResult>(context, kind, result!));
            RefreshList(context, dispatcher);
        }
    }

    private static async Task<(bool Success, TResult? Result)> TryExecute<TResult>(string context, FeatureSchemaParameterActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        try
        {
            TResult result = await operation();
            return (true, result);
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new SchemaParameterFailedAction(context, kind, ex.Message));
            return (false, default);
        }
    }

    private void RefreshList(string context, IDispatcher dispatcher)
    {
        if (state.Value.TryGetRequest(context, out RequestSchemaParameterList? request))
        {
            dispatcher.Dispatch(new SchemaParameterListAction(context, request));
        }
    }
}
