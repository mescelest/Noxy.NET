using Fluxor;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeDispatcher : IDispatcher
{
    private readonly Lock _lock = new();
    private readonly List<object> _actionList = [];

    public event EventHandler<ActionDispatchedEventArgs>? ActionDispatched;

    // Every action dispatched, in order.
    public List<object> ActionList
    {
        get
        {
            lock (_lock) return [.. _actionList];
        }
    }

    public void Dispatch(object action)
    {
        lock (_lock) _actionList.Add(action);

        ActionDispatched?.Invoke(this, new(action));
    }

    public List<TAction> Of<TAction>()
    {
        return ActionList.OfType<TAction>().ToList();
    }

    public void Clear()
    {
        lock (_lock) _actionList.Clear();
    }
}
