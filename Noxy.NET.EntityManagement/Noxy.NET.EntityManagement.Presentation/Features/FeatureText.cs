using Fluxor;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Requests.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Presentation.Features;

[FeatureState]
public record FeatureTextState
{
    public IReadOnlyDictionary<string, string> ResolvedTextCollection { get; init; } = new Dictionary<string, string>();
    public Dictionary<string, HashSet<string>> PendingByScope { get; init; } = [];

    public bool IsScopeLoading(string scope) => PendingByScope.TryGetValue(scope, out HashSet<string>? set) && set.Count > 0;
}

public static class FeatureTextReducers
{
    [ReducerMethod]
    public static FeatureTextState ReduceRequestKey(FeatureTextState state, RequestTextKeyAction action)
    {
        string key = action.Key;
        string scope = action.Scope;

        if (state.PendingByScope.TryGetValue(scope, out HashSet<string>? set) && set.Contains(key)) return state;

        Dictionary<string, HashSet<string>> pendingByScope = state.PendingByScope.ToDictionary(kv => kv.Key, kv => new HashSet<string>(kv.Value));
        pendingByScope[scope] = [..set ?? [], key];

        return state with
        {
            PendingByScope = pendingByScope
        };
    }

    [ReducerMethod]
    public static FeatureTextState ReduceCompleted(FeatureTextState state, TextCompletedAction action)
    {
        Dictionary<string, string> merged = state.ResolvedTextCollection.ToDictionary(x => x.Key, x => x.Value);
        foreach (KeyValuePair<string, string> kv in action.ResolvedTexts)
        {
            merged[kv.Key] = kv.Value;
        }

        return state with { ResolvedTextCollection = merged, PendingByScope = [] };
    }

    public record RequestTextKeyAction(string Key, string Scope = "");

    public record TextCompletedAction(Dictionary<string, string> ResolvedTexts);
}

public class FeatureTextEffects(IState<FeatureTextState> state, APIHttpClient http, IDebouncerService debouncer)
{
    private Dictionary<string, int> _lastPendingCount = [];

    [EffectMethod]
    public Task Handle(FeatureTextReducers.RequestTextKeyAction action, IDispatcher dispatcher)
    {
        if (!state.Value.PendingByScope.TryGetValue(action.Scope, out HashSet<string>? set) || !_lastPendingCount.TryGetValue(action.Scope, out int last) || set.Count == last) return Task.CompletedTask;

        string scope = $"{nameof(FeatureTextReducers)}.{nameof(FeatureTextReducers.RequestTextKeyAction)}:{action.Scope}";
        _lastPendingCount[action.Scope] = set.Count;
        debouncer.Debounce(() => ResolveInternal(action.Scope, dispatcher), scope);
        return Task.CompletedTask;
    }

    private async Task ResolveInternal(string scope, IDispatcher dispatcher)
    {
        if (!state.Value.PendingByScope.TryGetValue(scope, out HashSet<string>? set) || set.Count == 0) return;

        string[] snapshot = [.. set];

        RequestDataParameterTextResolveList request = new() { SchemaIdentifierList = snapshot };
        ResponseDataParameterResolveList response = await http.SendRequest(request);
        dispatcher.Dispatch(new FeatureTextReducers.TextCompletedAction(response.Value));
    }
}
