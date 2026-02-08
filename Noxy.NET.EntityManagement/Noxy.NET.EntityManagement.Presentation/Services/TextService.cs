using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Noxy.NET.EntityManagement.Presentation.Features;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class TextService(IServiceProvider provider)
{
    private IState<FeatureTextState> State => field ??= provider.GetRequiredService<IState<FeatureTextState>>();
    private IDispatcher Dispatcher => field ??= provider.GetRequiredService<IDispatcher>();

    public string Get(string? key, string? scope = null)
    {
        if (key == null) return "[KEY MISSING]";

        FeatureTextState s = State.Value;
        if (s.ResolvedTextCollection.TryGetValue(key, out string? value)) return value;
        if (!s.PendingKeys.Contains(key)) Dispatcher.Dispatch(new FeatureTextReducers.RequestTextKeyAction(key, scope));
        return string.Empty;
    }
}
