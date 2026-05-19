using System.Collections.Immutable;
using Fluxor;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions.Features;

public abstract record BaseFeatureParameterState
{
    private ImmutableDictionary<string, string?> _collection = ImmutableDictionary<string, string?>.Empty;
    private ImmutableDictionary<string, ImmutableHashSet<string>> _pending = ImmutableDictionary<string, ImmutableHashSet<string>>.Empty;
    private ImmutableHashSet<string> _resolved = ImmutableHashSet<string>.Empty;

    public IReadOnlyDictionary<string, string?> Collection => _collection;
    public IReadOnlyDictionary<string, ImmutableHashSet<string>> Pending => _pending;
    public IReadOnlySet<string> Resolved => _resolved;

    public bool IsScopeLoading(string scope) => _pending.TryGetValue(scope, out ImmutableHashSet<string>? set) && !set.IsEmpty;

    public BaseFeatureParameterState WithValues(Dictionary<string, string?>? values) => this with
    {
        _collection = values != null ? ImmutableDictionary.CreateRange(values) : ImmutableDictionary<string, string?>.Empty,
        _pending = _pending,
        _resolved = _resolved
    };

    public BaseFeatureParameterState WithPending(Dictionary<string, HashSet<string>>? pending) => this with
    {
        _collection = _collection,
        _pending = pending != null ? ConvertPending(pending) : ImmutableDictionary<string, ImmutableHashSet<string>>.Empty,
        _resolved = _resolved
    };

    public BaseFeatureParameterState WithResolved(HashSet<string>? resolved) => this with
    {
        _collection = _collection,
        _pending = _pending,
        _resolved = resolved != null ? ImmutableHashSet.CreateRange(resolved) : ImmutableHashSet<string>.Empty
    };

    public BaseFeatureParameterState WithValues(ImmutableDictionary<string, string?> values) => this with
    {
        _collection = values,
        _pending = _pending,
        _resolved = _resolved
    };

    public BaseFeatureParameterState WithPending(ImmutableDictionary<string, ImmutableHashSet<string>> pending) => this with
    {
        _collection = _collection,
        _pending = pending,
        _resolved = _resolved
    };

    public BaseFeatureParameterState WithResolved(ImmutableHashSet<string> resolved) => this with
    {
        _collection = _collection,
        _pending = _pending,
        _resolved = resolved
    };

    private static ImmutableDictionary<string, ImmutableHashSet<string>> ConvertPending(Dictionary<string, HashSet<string>> source)
    {
        ImmutableDictionary<string, ImmutableHashSet<string>> result = ImmutableDictionary<string, ImmutableHashSet<string>>.Empty;
        foreach (KeyValuePair<string, HashSet<string>> kv in source)
        {
            ImmutableHashSet<string> set = ImmutableHashSet.CreateRange(kv.Value);
            result = result.SetItem(kv.Key, set);
        }

        return result;
    }
}

public abstract class ParameterReducersBase<TState> where TState : BaseFeatureParameterState
{
    [ReducerMethod]
    public static TState ReduceLoadKey(TState state, LoadKey action)
    {
        if (state.Resolved.Contains(action.Key) || state.Pending.TryGetValue(action.Scope, out ImmutableHashSet<string>? setExisting) && setExisting.Contains(action.Key))
        {
            return state;
        }

        ImmutableDictionary<string, ImmutableHashSet<string>> pending = state.Pending.Aggregate(ImmutableDictionary<string, ImmutableHashSet<string>>.Empty, (dictionary, kv) => dictionary.Add(kv.Key, kv.Value));
        ImmutableHashSet<string> previous = pending.TryGetValue(action.Scope, out ImmutableHashSet<string>? existing) ? existing : ImmutableHashSet<string>.Empty;
        ImmutableHashSet<string> current = previous.Add(action.Key);
        pending = pending.SetItem(action.Scope, current);

        return (TState)state.WithPending(pending);
    }

    [ReducerMethod]
    public static TState ReduceResolvePending(TState state, ResolvePending action)
    {
        if (!state.Pending.TryGetValue(action.Scope, out ImmutableHashSet<string>? previous))
        {
            return state;
        }

        ImmutableDictionary<string, ImmutableHashSet<string>> pending = state.Pending.Aggregate(ImmutableDictionary<string, ImmutableHashSet<string>>.Empty, (dictionary, kv) => dictionary.Add(kv.Key, kv.Value));
        ImmutableHashSet<string> current = action.Set.Aggregate(previous, (set, item) => set.Remove(item));
        pending = pending.SetItem(action.Scope, current);

        return (TState)state.WithPending(pending);
    }

    [ReducerMethod]
    public static TState ReduceRestorePending(TState state, RestorePending action)
    {
        ImmutableDictionary<string, ImmutableHashSet<string>> pending = state.Pending.Aggregate(ImmutableDictionary<string, ImmutableHashSet<string>>.Empty, (dictionary, kv) => dictionary.Add(kv.Key, kv.Value));
        ImmutableHashSet<string> previous = pending.TryGetValue(action.Scope, out ImmutableHashSet<string>? value) ? value : ImmutableHashSet<string>.Empty;
        ImmutableHashSet<string> current = action.Set.Aggregate(previous, (set, item) => set.Add(item));
        pending = pending.SetItem(action.Scope, current);

        return (TState)state.WithPending(pending);
    }

    [ReducerMethod]
    public static TState ReduceSuccess(TState state, Success action)
    {
        ImmutableDictionary<string, string?> merged = state.Collection as ImmutableDictionary<string, string?> ?? ImmutableDictionary.CreateRange(state.Collection);
        merged = action.ResolvedValues.Aggregate(merged, (dictionary, kv) => dictionary.SetItem(kv.Key, kv.Value));

        return (TState)state.WithValues(merged);
    }

    [ReducerMethod]
    public static TState ReduceMarkResolved(TState state, MarkResolved action)
    {
        ImmutableHashSet<string> resolved = state.Resolved as ImmutableHashSet<string> ?? ImmutableHashSet.CreateRange(state.Resolved);
        resolved = action.Set.Aggregate(resolved, (set, item) => set.Add(item));

        return (TState)state.WithResolved(resolved);
    }

    public record LoadKey(string Key, string Scope = "");

    public record ResolvePending(string[] Set, string Scope);

    public record RestorePending(string[] Set, string Scope);

    public record Success(Dictionary<string, string?> ResolvedValues);

    public record MarkResolved(string[] Set);
}

public abstract class ParameterEffectsBase<TState>(IState<TState> state, IDebouncerService debouncer) where TState : BaseFeatureParameterState
{
    private readonly Dictionary<string, int> _collectionScopeVersion = new();
    protected abstract Task<Dictionary<string, string?>> Resolve(IReadOnlyCollection<string> keys);

    [EffectMethod]
    public Task Handle(ParameterReducersBase<TState>.LoadKey action, IDispatcher dispatcher)
    {
        if (!state.Value.Pending.TryGetValue(action.Scope, out ImmutableHashSet<string> _)) return Task.CompletedTask;

        int version = _collectionScopeVersion.TryGetValue(action.Scope, out int last) ? last + 1 : 1;
        _collectionScopeVersion[action.Scope] = version;

        string debounceKey = $"{typeof(TState).Name}:{action.Scope}:{version}";
        debouncer.Debounce(() => ResolveInternal(action.Scope, version, dispatcher), debounceKey);

        return Task.CompletedTask;
    }

    private async Task ResolveInternal(string scope, int version, IDispatcher dispatcher)
    {
        if (!_collectionScopeVersion.TryGetValue(scope, out int current) || current != version) return;
        if (!state.Value.Pending.TryGetValue(scope, out ImmutableHashSet<string>? set) || set.IsEmpty) return;

        string[] snapshot = new string[set.Count];
        int index = 0;
        foreach (string item in set)
        {
            snapshot[index++] = item;
        }

        try
        {
            dispatcher.Dispatch(new ParameterReducersBase<TState>.ResolvePending(snapshot, scope));

            Dictionary<string, string?> resolved = await Resolve(snapshot);

            dispatcher.Dispatch(new ParameterReducersBase<TState>.Success(resolved));
            dispatcher.Dispatch(new ParameterReducersBase<TState>.MarkResolved(snapshot));
        }
        catch
        {
            dispatcher.Dispatch(new ParameterReducersBase<TState>.RestorePending(snapshot, scope));
        }
    }
}
