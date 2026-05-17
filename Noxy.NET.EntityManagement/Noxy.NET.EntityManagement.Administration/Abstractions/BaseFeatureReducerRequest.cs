using Noxy.NET.EntityManagement.Domain.Models;

namespace Noxy.NET.EntityManagement.Administration.Abstractions;

public abstract class BaseFeatureReducerRequest<TState, TKind> where TKind : struct, Enum where TState : BaseFeatureStateRequest<TKind>
{
    public record SuccessAction(string Scope, TKind Kind);

    public record SuccessAction<T>(string Scope, TKind Kind, T Value);

    public record FailureAction(string Scope, TKind Kind, string Error);

    protected static Dictionary<TDictionaryKey, TValue> Set<TDictionaryKey, TValue>(Dictionary<TDictionaryKey, TValue> source, TDictionaryKey key, TValue value) where TDictionaryKey : notnull
    {
        return source.TryGetValue(key, out TValue? existing) && EqualityComparer<TValue>.Default.Equals(existing, value) ? source : new(source) { [key] = value };
    }

    protected static TState StartAction(TState state, string scope, TKind kind, Func<TState, TState>? configure = null)
    {
        FeatureKey<TKind> key = new(scope, kind);

        TState next = state with
        {
            Loading = Set(state.Loading, key, true),
            Error = Set(state.Error, key, null),
        };

        return configure is null ? next : configure(next);
    }

    protected static TState HandleSuccessAction(TState state, SuccessAction action, Func<TState, TState>? configure = null)
    {
        FeatureKey<TKind> key = new(action.Scope, action.Kind);

        TState next = state with
        {
            Loading = Set(state.Loading, key, false),
        };

        return configure is null ? next : configure(next);
    }

    protected static TState HandleSuccessAction<TValue>(TState state, SuccessAction<TValue> action, Func<TState, TValue, TState>? configure = null)
    {
        FeatureKey<TKind> key = new(action.Scope, action.Kind);

        TState next = state with
        {
            Loading = Set(state.Loading, key, false),
        };

        return configure is null ? next : configure(next, action.Value);
    }

    protected static TState HandleFailureAction(TState state, FailureAction action, Func<TState, string, TState>? configure = null)
    {
        FeatureKey<TKind> key = new(action.Scope, action.Kind);

        TState next = state with
        {
            Loading = Set(state.Loading, key, false),
            Error = Set(state.Error, key, action.Error),
        };

        return configure is null ? next : configure(next, action.Error);
    }
}
