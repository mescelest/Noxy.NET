using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaParameterReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public readonly record struct FeatureSchemaParameterKey(string Scope, FeatureSchemaParameterActionKind Kind);

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

    public bool TryGetLoading(string scope, FeatureSchemaParameterActionKind kind, out bool value) => Loading.TryGetValue(new(scope, kind), out value);
    public bool TryGetError(string scope, FeatureSchemaParameterActionKind kind, out string? error) => Error.TryGetValue(new(scope, kind), out error);

    public bool TryGetList(string scope, [NotNullWhen(true)] out IReadOnlyList<EntitySchemaParameter.Discriminator>? list) => Cache.TryGetValue(scope, out list);
    public bool TryGetRequest(string scope, [NotNullWhen(true)] out RequestSchemaParameterList? request) => Request.TryGetValue(scope, out request);
}

public static class FeatureSchemaParameterReducers
{
    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceList(FeatureSchemaParameterState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureSchemaParameterActionKind.List, (next) => next with { Request = Set(next.Request, action.Scope, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceListSuccess(FeatureSchemaParameterState state, SuccessAction<List<EntitySchemaParameter.Discriminator>> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Cache = Set(next.Cache, action.Scope, value) });

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceListFailure(FeatureSchemaParameterState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceAddText(FeatureSchemaParameterState state, AddTextAction action) => StartAction(state, action.Scope, FeatureSchemaParameterActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceAddTextSuccess(FeatureSchemaParameterState state, SuccessAction<EntitySchemaParameterText> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceAddTextFailure(FeatureSchemaParameterState state, FailureAction action) => HandleFailureAction(state, action);

    private static Dictionary<TKey, TValue> Set<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key, TValue value) where TKey : notnull
    {
        return source.TryGetValue(key, out TValue? existing) && EqualityComparer<TValue>.Default.Equals(existing, value) ? source : new(source) { [key] = value };
    }

    private static FeatureSchemaParameterState StartAction(FeatureSchemaParameterState state, string scope, FeatureSchemaParameterActionKind kind, Func<FeatureSchemaParameterState, FeatureSchemaParameterState>? cb = null)
    {
        FeatureSchemaParameterKey key = new(scope, kind);
        FeatureSchemaParameterState newState = state with
        {
            Loading = Set(state.Loading, key, true),
            Error = Set(state.Error, key, null)
        };

        return cb != null ? cb(newState) : newState;
    }

    private static FeatureSchemaParameterState HandleSuccessAction<T>(FeatureSchemaParameterState state, SuccessAction<T> action, Func<FeatureSchemaParameterState, T, FeatureSchemaParameterState>? cb = null)
    {
        FeatureSchemaParameterKey key = new(action.Scope, action.Kind);
        FeatureSchemaParameterState newState = state with
        {
            Loading = Set(state.Loading, key, false)
        };

        return cb != null ? cb(newState, action.Value) : newState;
    }

    private static FeatureSchemaParameterState HandleFailureAction(FeatureSchemaParameterState state, FailureAction action, Func<FeatureSchemaParameterState, FailureAction, FeatureSchemaParameterState>? cb = null)
    {
        FeatureSchemaParameterKey key = new(action.Scope, action.Kind);
        FeatureSchemaParameterState newState = state with
        {
            Loading = Set(state.Loading, key, false),
            Error = Set(state.Error, key, action.Error)
        };

        return cb != null ? cb(newState, action) : newState;
    }

    public record ListAction(string Scope, RequestSchemaParameterList Request);

    public record AddTextAction(string Scope, RequestSchemaParameterTextCreate Request);

    public record SuccessAction<T>(string Scope, FeatureSchemaParameterActionKind Kind, T Value);

    public record FailureAction(string Scope, FeatureSchemaParameterActionKind Kind, string Error);
}

public class FeatureSchemaParameterEffects(APIHttpClient client, IState<FeatureSchemaParameterState> state)
{
    [EffectMethod]
    public Task Handle(ListAction action, IDispatcher dispatcher) => Execute(action.Scope, FeatureSchemaParameterActionKind.List, dispatcher, async () => (await client.SendRequest(action.Request)).Value);

    [EffectMethod]
    public Task Handle(AddTextAction action, IDispatcher dispatcher) => ExecuteWithRefresh(action.Scope, FeatureSchemaParameterActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);

    private static async Task Execute<TResult>(string scope, FeatureSchemaParameterActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(scope, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SuccessAction<TResult>(scope, kind, result!));
        }
    }

    private async Task ExecuteWithRefresh<TResult>(string scope, FeatureSchemaParameterActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(scope, kind, dispatcher, operation);
        if (success)
        {
            dispatcher.Dispatch(new SuccessAction<TResult>(scope, kind, result!));
            RefreshList(scope, dispatcher);
        }
    }

    private static async Task<(bool Success, TResult? Result)> TryExecute<TResult>(string scope, FeatureSchemaParameterActionKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
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
        if (state.Value.TryGetRequest(scope, out RequestSchemaParameterList? request))
        {
            dispatcher.Dispatch(new ListAction(scope, request));
        }
    }
}
