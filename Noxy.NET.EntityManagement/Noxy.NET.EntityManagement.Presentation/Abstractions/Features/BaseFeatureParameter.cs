using System.Collections.Immutable;
using Fluxor;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions.Features;

public abstract record BaseFeatureParameterState
{
    public ImmutableDictionary<string, string?> Collection { get; init; } = ImmutableDictionary<string, string?>.Empty;

    public ImmutableDictionary<string, ImmutableHashSet<string>> Pending { get; init; } = ImmutableDictionary<string, ImmutableHashSet<string>>.Empty;

    public ImmutableHashSet<string> Resolved { get; init; } = ImmutableHashSet<string>.Empty;

    public bool IsScopeLoading(string scope) => Pending.TryGetValue(scope, out ImmutableHashSet<string>? set) && set.Count > 0;
}

public abstract class ParameterReducersBase<TState> where TState : BaseFeatureParameterState
{
    [ReducerMethod]
    public static TState ReduceLoadKey(TState state, LoadKey action)
    {
        if (state.Resolved.Contains(action.Key) || (state.Pending.TryGetValue(action.Scope, out ImmutableHashSet<string>? set) && set.Contains(action.Key))) return state;

        ImmutableHashSet<string> updatedSet = (set ?? ImmutableHashSet<string>.Empty).Add(action.Key);
        ImmutableDictionary<string, ImmutableHashSet<string>> pending = state.Pending.SetItem(action.Scope, updatedSet);

        return state with { Pending = pending };
    }


    [ReducerMethod]
    public static TState ReduceResolvePending(TState state, ResolvePending action)
    {
        if (!state.Pending.TryGetValue(action.Scope, out ImmutableHashSet<string>? set)) return state;

        ImmutableHashSet<string> updatedSet = set.Except(action.Set);
        ImmutableDictionary<string, ImmutableHashSet<string>> pending = state.Pending.SetItem(action.Scope, updatedSet);
        ImmutableHashSet<string> resolved = state.Resolved.Union(action.Set);

        return state with { Pending = pending, Resolved = resolved };
    }

    [ReducerMethod]
    public static TState ReduceRestorePending(TState state, RestorePending action)
    {
        ImmutableHashSet<string> set = state.Pending.TryGetValue(action.Scope, out ImmutableHashSet<string>? existing) ? existing : ImmutableHashSet<string>.Empty;
        ImmutableHashSet<string> updatedSet = set.Union(action.Set);
        ImmutableDictionary<string, ImmutableHashSet<string>> pending = state.Pending.SetItem(action.Scope, updatedSet);
        ImmutableHashSet<string> resolved = state.Resolved.Except(action.Set);

        return state with { Pending = pending, Resolved = resolved };
    }

    [ReducerMethod]
    public static TState ReduceSuccess(TState state, Success action)
    {
        ImmutableDictionary<string, string?> merged = state.Collection.SetItems(action.ResolvedValues);
        return state with { Collection = merged };
    }

    public record LoadKey(string Key, string Scope = "");

    public record ResolvePending(string[] Set, string Scope);

    public record RestorePending(string[] Set, string Scope);

    public record Success(Dictionary<string, string?> ResolvedValues);
}

public abstract class ParameterEffectsBase<TState>(IState<TState> state, IDebouncerService debouncer)
    where TState : BaseFeatureParameterState
{
    private readonly Dictionary<string, int> _lastPendingCount = [];

    protected abstract Task<Dictionary<string, string?>> Resolve(IReadOnlyCollection<string> keys);

    [EffectMethod]
    public Task Handle(ParameterReducersBase<TState>.LoadKey action, IDispatcher dispatcher)
    {
        if (!state.Value.Pending.TryGetValue(action.Scope, out ImmutableHashSet<string>? set)) return Task.CompletedTask;

        int currentCount = set.Count;
        if (!_lastPendingCount.TryGetValue(action.Scope, out int lastCount)) _lastPendingCount[action.Scope] = 0;
        if (currentCount == lastCount) return Task.CompletedTask;

        _lastPendingCount[action.Scope] = currentCount;
        string debounceKey = $"{typeof(TState).Name}:{action.Scope}";
        debouncer.Debounce(() => ResolveInternal(action.Scope, dispatcher), debounceKey, 10);

        return Task.CompletedTask;
    }

    private async Task ResolveInternal(string scope, IDispatcher dispatcher)
    {
        if (!state.Value.Pending.TryGetValue(scope, out ImmutableHashSet<string>? set) || set.Count == 0) return;

        string[] snapshot = [.. set];

        try
        {
            dispatcher.Dispatch(new ParameterReducersBase<TState>.ResolvePending(snapshot, scope));

            Dictionary<string, string?> resolved = await Resolve(snapshot);
            dispatcher.Dispatch(new ParameterReducersBase<TState>.Success(resolved));
        }
        catch
        {
            dispatcher.Dispatch(new ParameterReducersBase<TState>.RestorePending(snapshot, scope));
        }
    }
}
