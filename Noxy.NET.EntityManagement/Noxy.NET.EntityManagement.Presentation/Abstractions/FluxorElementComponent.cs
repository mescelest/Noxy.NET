using System.Reflection;
using Fluxor;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions;

public abstract class FluxorElementComponent : ElementComponent, IDisposable
{
    private readonly List<IDisposable> _subscriptions = new();

    public void Dispose()
    {
        foreach (IDisposable sub in _subscriptions) sub.Dispose();

        _subscriptions.Clear();
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        AutoObserveStates();
    }

    private void AutoObserveStates()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Find all fields and properties of type IState<T>
        IEnumerable<MemberInfo> members = GetType()
            .GetMembers(flags)
            .Where(m => (m is FieldInfo fi && IsIState(fi.FieldType)) || (m is PropertyInfo pi && IsIState(pi.PropertyType)));

        foreach (MemberInfo member in members)
        {
            object? value = member switch
            {
                FieldInfo fi => fi.GetValue(this),
                PropertyInfo pi => pi.GetValue(this),
                _ => null
            };

            if (value != null) SubscribeToState(value);
        }
    }

    private static bool IsIState(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IState<>);

    private void SubscribeToState(object stateObj)
    {
        EventInfo? eventInfo = stateObj.GetType().GetEvent("StateChanged");
        if (eventInfo == null) return;

        EventHandler handler = (_, _) => InvokeAsync(StateHasChanged);
        eventInfo.AddEventHandler(stateObj, handler);
        _subscriptions.Add(new Subscription(() => eventInfo.RemoveEventHandler(stateObj, handler)));
    }

    private sealed class Subscription(Action unsubscribe) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            unsubscribe();
        }
    }
}
