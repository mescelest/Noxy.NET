using Fluxor;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions;

public abstract class FluxorElementComponent : ElementComponent, IDisposable
{
    private readonly List<(object State, Delegate Handler)> _subscriptions = [];

    public void Dispose()
    {
        foreach ((object State, Delegate Handler) sub in _subscriptions)
        {
            if (sub.State is IState<object> generic)
                generic.StateChanged -= (EventHandler)sub.Handler;
            else
                ((dynamic)sub.State).StateChanged -= (EventHandler)sub.Handler;
        }

        _subscriptions.Clear();

        GC.SuppressFinalize(this);
    }

    protected void Observe<T>(IState<T> state)
    {
        EventHandler handler = OnStateChanged;
        state.StateChanged += handler;
        _subscriptions.Add((state, handler));
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }
}
