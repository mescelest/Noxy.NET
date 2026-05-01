using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Requests.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Presentation.Features;

[FeatureState]
public record FeatureTextState
{
    public IReadOnlyDictionary<string, string> TextCollection { get; init; } = new Dictionary<string, string>();
    public Dictionary<string, HashSet<string>> Pending { get; init; } = [];
    public HashSet<string> Resolved { get; set; } = [];

    public bool IsScopeLoading(string scope) => Pending.TryGetValue(scope, out HashSet<string>? set) && set.Count > 0;
}

public static class FeatureTextReducers
{
    [ReducerMethod]
    public static FeatureTextState ReduceLoadKey(FeatureTextState state, LoadKeyAction action)
    {
        if (state.Resolved.Contains(action.Key) || state.Pending.TryGetValue(action.Scope, out HashSet<string>? set) && set.Contains(action.Key)) return state;

        Dictionary<string, HashSet<string>> pending = state.Pending.ToDictionary();
        pending[action.Scope] = [.. set ?? [], action.Key];

        return state with
        {
            Pending = pending
        };
    }

    [ReducerMethod]
    public static FeatureTextState ReduceResolvePending(FeatureTextState state, ResolvePendingAction action)
    {
        if (!state.Pending.TryGetValue(action.Scope, out HashSet<string>? set)) return state;

        Dictionary<string, HashSet<string>> pending = state.Pending.ToDictionary(kv => kv.Key, kv => new HashSet<string>(kv.Value));
        pending[action.Scope] = [.. set.Where(x => !action.Set.Contains(x))];

        HashSet<string> resolved = [.. state.Resolved, .. action.Set];

        return state with { Pending = pending, Resolved = resolved };
    }

    [ReducerMethod]
    public static FeatureTextState ReduceRestorePending(FeatureTextState state, RestorePendingAction action)
    {
        Dictionary<string, HashSet<string>> pending = state.Pending.ToDictionary(kv => kv.Key, kv => new HashSet<string>(kv.Value));
        HashSet<string> set = state.Pending.TryGetValue(action.Scope, out HashSet<string>? value) ? value : [];
        pending[action.Scope] = [.. set, .. action.Set];

        HashSet<string> resolved = [..state.Resolved.Where(x => !action.Set.Contains(x))];

        return state with { Pending = pending, Resolved = resolved };
    }

    [ReducerMethod]
    public static FeatureTextState ReduceSuccess(FeatureTextState state, SuccessAction action)
    {
        Dictionary<string, string> merged = state.TextCollection.ToDictionary(x => x.Key, x => x.Value);
        foreach (KeyValuePair<string, string> kv in action.ResolvedTexts)
        {
            merged[kv.Key] = kv.Value;
        }

        return state with { TextCollection = merged };
    }

    public record LoadKeyAction(string Key, string Scope = "");

    public record ResolvePendingAction(string[] Set, string Scope);

    public record RestorePendingAction(string[] Set, string Scope);

    public record SuccessAction(Dictionary<string, string> ResolvedTexts);
}

public class FeatureTextEffects(IState<FeatureTextState> state, APIHttpClient http, IDebouncerService debouncer)
{
    private readonly Dictionary<string, int> _lastPendingCount = [];

    [EffectMethod]
    public async Task Handle(FeatureTextReducers.LoadKeyAction action, IDispatcher dispatcher)
    {
        if (!state.Value.Pending.TryGetValue(action.Scope, out var set)) return;
        int currentCount = set.Count;

        if (!_lastPendingCount.TryGetValue(action.Scope, out int lastCount)) _lastPendingCount[action.Scope] = 0;
        if (currentCount == lastCount) return;

        _lastPendingCount[action.Scope] = currentCount;

        string debounceKey = $"{nameof(FeatureTextReducers)}.{nameof(FeatureTextReducers.LoadKeyAction)}:{action.Scope}";
        debouncer.Debounce(() => ResolveInternal(action.Scope, dispatcher), debounceKey);
    }

    private async Task ResolveInternal(string scope, IDispatcher dispatcher)
    {
        if (!state.Value.Pending.TryGetValue(scope, out var set) || set.Count == 0) return;

        string[] snapshot = [.. set];

        try
        {
            dispatcher.Dispatch(new FeatureTextReducers.ResolvePendingAction(snapshot, scope));

            RequestDataParameterTextResolveList request = new() { SchemaIdentifierList = snapshot };
            ResponseDataParameterResolveList response = await http.SendRequest(request);

            dispatcher.Dispatch(new FeatureTextReducers.SuccessAction(response.Value));
        }
        catch
        {
            dispatcher.Dispatch(new FeatureTextReducers.RestorePendingAction(snapshot, scope));
            // Should attempt to get the restored pending too.
        }
    }
}
