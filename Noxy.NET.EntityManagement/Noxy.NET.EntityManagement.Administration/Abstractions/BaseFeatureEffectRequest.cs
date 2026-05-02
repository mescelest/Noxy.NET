using Fluxor;

namespace Noxy.NET.EntityManagement.Administration.Abstractions;

public abstract class BaseFeatureEffectRequest<TState, TKind> where TState : BaseFeatureStateRequest<TKind> where TKind : struct, Enum
{
    protected static async Task Execute<TResult>(string scope, TKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(scope, kind, dispatcher, operation);

        if (success)
        {
            dispatcher.Dispatch(new BaseFeatureReducerRequest<TState, TKind>.SuccessAction<TResult>(scope, kind, result!));
        }
    }

    protected async Task ExecuteWithRefresh<TResult>(string scope, TKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(scope, kind, dispatcher, operation);

        if (success)
        {
            dispatcher.Dispatch(new BaseFeatureReducerRequest<TState, TKind>.SuccessAction<TResult>(scope, kind, result!));
            RefreshList(scope, dispatcher);
        }
    }

    private static async Task<(bool Success, TResult? Result)> TryExecute<TResult>(string scope, TKind kind, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        try
        {
            TResult result = await operation();
            return (true, result);
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new BaseFeatureReducerRequest<TState, TKind>.FailureAction(scope, kind, ex.Message));
            return (false, default);
        }
    }

    protected abstract void RefreshList(string scope, IDispatcher dispatcher);
}
