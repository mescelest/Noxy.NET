using Fluxor;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions.Features;

public abstract record BaseFeatureParameterState
{
    public IReadOnlyDictionary<string, string?> Collection { get; init; } = new Dictionary<string, string?>();
    public IReadOnlyDictionary<string, HashSet<string>> Pending { get; init; } = new Dictionary<string, HashSet<string>>();
    public HashSet<string> Resolved { get; init; } = [];

    public bool IsScopeLoading(string scope) => Pending.TryGetValue(scope, out HashSet<string>? set) && set.Count > 0;
}

public abstract class ParameterReducersBase<TState> where TState : BaseFeatureParameterState
{
    [ReducerMethod]
    public static TState ReduceLoadKey(TState state, LoadKey action)
    {
        if (state.Resolved.Contains(action.Key) || state.Pending.TryGetValue(action.Scope, out HashSet<string>? set) && set.Contains(action.Key))
        {
            return state;
        }

        HashSet<string> current = set is null ? [action.Key] : [..set, action.Key];
        Dictionary<string, HashSet<string>> pending = new(state.Pending) { [action.Scope] = current };

        return state with { Pending = pending };
    }

    [ReducerMethod]
    public static TState ReduceResolvePending(TState state, ResolvePending action)
    {
        if (!state.Pending.TryGetValue(action.Scope, out HashSet<string>? set)) return state;

        HashSet<string> current = [.. set.Where(x => !action.Set.Contains(x))];
        Dictionary<string, HashSet<string>> pending = new(state.Pending) { [action.Scope] = current };
        HashSet<string> resolved = [.. state.Resolved, .. action.Set];

        return state with { Pending = pending, Resolved = resolved };
    }

    [ReducerMethod]
    public static TState ReduceRestorePending(TState state, RestorePending action)
    {
        HashSet<string> set = state.Pending.TryGetValue(action.Scope, out HashSet<string>? existing) ? existing : [];

        HashSet<string> current = [.. set, .. action.Set];
        Dictionary<string, HashSet<string>> pending = new(state.Pending) { [action.Scope] = current };
        HashSet<string> resolved = [.. state.Resolved.Where(x => !action.Set.Contains(x))];

        return state with { Pending = pending, Resolved = resolved };
    }

    [ReducerMethod]
    public static TState ReduceSuccess(TState state, Success action)
    {
        Dictionary<string, string?> merged = new(state.Collection);
        foreach (KeyValuePair<string, string?> kv in action.ResolvedValues)
        {
            merged[kv.Key] = kv.Value;
        }

        return state with { Collection = merged };
    }

    public record LoadKey(string Key, string Scope = "");

    public record ResolvePending(string[] Set, string Scope);

    public record RestorePending(string[] Set, string Scope);

    public record Success(Dictionary<string, string?> ResolvedValues);
}

public abstract class ParameterEffectsBase<TState>(IState<TState> state, IDebouncerService debouncer) where TState : BaseFeatureParameterState
{
    private readonly Dictionary<string, int> _lastPendingCount = [];

    protected abstract Task<Dictionary<string, string?>> Resolve(IReadOnlyCollection<string> keys);

    [EffectMethod]
    public Task Handle(ParameterReducersBase<TState>.LoadKey action, IDispatcher dispatcher)
    {
        if (!state.Value.Pending.TryGetValue(action.Scope, out HashSet<string>? set)) return Task.CompletedTask;

        int currentCount = set.Count;
        if (!_lastPendingCount.TryGetValue(action.Scope, out int lastCount))
        {
            _lastPendingCount[action.Scope] = 0;
        }

        if (currentCount == lastCount) return Task.CompletedTask;
        _lastPendingCount[action.Scope] = currentCount;

        string debounceKey = $"{typeof(TState).Name}:{action.Scope}";
        debouncer.Debounce(() => ResolveInternal(action.Scope, dispatcher), debounceKey, 10);

        return Task.CompletedTask;
    }

    private async Task ResolveInternal(string scope, IDispatcher dispatcher)
    {
        if (!state.Value.Pending.TryGetValue(scope, out HashSet<string>? set) || set.Count == 0) return;

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
