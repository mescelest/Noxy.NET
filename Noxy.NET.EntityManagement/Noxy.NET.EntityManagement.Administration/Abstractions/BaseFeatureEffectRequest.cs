using Fluxor;

namespace Noxy.NET.EntityManagement.Administration.Abstractions;

public abstract class BaseFeatureEffectRequest<TState, TKind> where TState : BaseFeatureStateRequest<TKind> where TKind : struct, Enum
{
    protected record ExecuteConfiguration(string Scope, TKind Kind, bool Refresh = true, bool Discard = true);

    protected async Task Execute<TResult>(ExecuteConfiguration config, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        (bool success, TResult? result) = await TryExecute(config, dispatcher, operation);

        if (success)
        {
            if (!config.Discard)
            {
                dispatcher.Dispatch(new BaseFeatureReducerRequest<TState, TKind>.SuccessAction<TResult>(config.Scope, config.Kind, result!));
            }
            else
            {
                dispatcher.Dispatch(new BaseFeatureReducerRequest<TState, TKind>.SuccessAction(config.Scope, config.Kind));
            }

            if (config.Refresh)
            {
                RefreshList(config.Scope, dispatcher);
            }
        }
    }

    private static async Task<(bool Success, TResult? Result)> TryExecute<TResult>(ExecuteConfiguration config, IDispatcher dispatcher, Func<Task<TResult>> operation)
    {
        try
        {
            TResult result = await operation();
            return (true, result);
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new BaseFeatureReducerRequest<TState, TKind>.FailureAction(config.Scope, config.Kind, ex.Message));
            return (false, default);
        }
    }

    protected abstract void RefreshList(string scope, IDispatcher dispatcher);
}
