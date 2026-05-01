using Fluxor;
using Noxy.NET.EntityManagement.Presentation.Features;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class TextService : IState<FeatureTextState>
{
    private readonly IState<FeatureTextState> _inner;
    private readonly IDispatcher _dispatcher;
    public FeatureTextState Value => _inner.Value;
    public event EventHandler? StateChanged;

    public TextService(IState<FeatureTextState> inner, IDispatcher dispatcher)
    {
        _inner = inner;
        _dispatcher = dispatcher;

        _inner.StateChanged += (_, _) => StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public string Get(string? key, string scope = "")
    {
        if (key == null) return "[KEY MISSING]";

        FeatureTextState state = Value;
        if (state.TextCollection.TryGetValue(key, out string? value)) return value;

        if (!state.Pending.TryGetValue(scope, out var set) || !set.Contains(key))
            _dispatcher.Dispatch(new FeatureTextReducers.LoadKeyAction(key, scope));

        return "Loading…";
    }
}
