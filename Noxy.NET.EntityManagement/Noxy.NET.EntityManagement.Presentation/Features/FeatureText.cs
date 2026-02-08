using Fluxor;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Presentation.Features;

[FeatureState]
public record FeatureTextState
{
    public IReadOnlyDictionary<string, string> ResolvedTextCollection { get; init; } = new Dictionary<string, string>();
    public HashSet<string> PendingKeys { get; init; } = [];
    public Dictionary<string, HashSet<string>> PendingByScope { get; init; } = [];

    public bool IsLoading => PendingKeys.Count > 0;
    public bool IsScopeLoading(string scope) => PendingByScope.TryGetValue(scope, out HashSet<string>? set) && set.Count > 0;
}

public static class FeatureTextReducers
{
    [ReducerMethod]
    public static FeatureTextState ReduceRequestKey(FeatureTextState state, RequestTextKeyAction action)
    {
        string key = action.Key;
        string? scope = action.Scope;

        bool pendingGlobally = state.PendingKeys.Contains(key);
        bool pendingInScope = scope is not null && state.PendingByScope.TryGetValue(scope, out HashSet<string>? scopedSet) && scopedSet.Contains(key);
        if (pendingGlobally && (scope is null || pendingInScope)) return state;

        HashSet<string> pending = [.. state.PendingKeys, key];
        if (scope is null) return state with { PendingKeys = pending };

        Dictionary<string, HashSet<string>> pendingByScope = state.PendingByScope.ToDictionary(kv => kv.Key, kv => new HashSet<string>(kv.Value));
        if (!pendingByScope.TryGetValue(scope, out HashSet<string>? set)) pendingByScope[scope] = set = [];

        set.Add(key);
        return state with
        {
            PendingKeys = pending,
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

        return state with { ResolvedTextCollection = merged, PendingKeys = [], PendingByScope = [] };
    }

    public record RequestTextKeyAction(string Key, string? Scope = null);

    public record TextCompletedAction(Dictionary<string, string> ResolvedTexts);
}

public class FeatureTextEffects(IState<FeatureTextState> state, APIHttpClient http, IDebouncerService debouncer)
{
    [EffectMethod]
    public Task Handle(FeatureTextReducers.RequestTextKeyAction action, IDispatcher dispatcher)
    {
        debouncer.Debounce(() => ResolveInternal(dispatcher));
        return Task.CompletedTask;
    }

    private async Task ResolveInternal(IDispatcher dispatcher)
    {
        string[] snapshot = state.Value.PendingKeys.ToArray();
        if (snapshot.Length == 0) return;

        RequestDataParameterTextResolveList request = new() { SchemaIdentifierList = snapshot };
        ResponseDataParameterResolveList response = await http.SendRequest(request);
        dispatcher.Dispatch(new FeatureTextReducers.TextCompletedAction(response.Value));
    }
}
