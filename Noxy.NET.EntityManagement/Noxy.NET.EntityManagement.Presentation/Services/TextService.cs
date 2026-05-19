using Fluxor;
using Noxy.NET.EntityManagement.Presentation.Abstractions.Features;
using Noxy.NET.EntityManagement.Presentation.Features;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class TextService : IState<FeatureTextState>
{
    private readonly IState<FeatureTextState> _state;
    private readonly IDispatcher _dispatcher;

    public FeatureTextState Value => _state.Value;

    public event EventHandler? StateChanged;

    public TextService(IState<FeatureTextState> state, IDispatcher dispatcher)
    {
        _state = state;
        _dispatcher = dispatcher;

        _state.StateChanged += (_, _) => StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public string Get(string? key, string scope = "")
    {
        if (key == null) return "[KEY MISSING]";

        if (Value.Collection.TryGetValue(key, out string? value)) return value ?? "[VALUE MISSING]";
        if (!Value.Pending.TryGetValue(scope, out HashSet<string>? set) || !set.Contains(key))
        {
            _dispatcher.Dispatch(new ParameterReducersBase<FeatureTextState>.LoadKey(key, scope));
        }

        return "Loading…";
    }
}
