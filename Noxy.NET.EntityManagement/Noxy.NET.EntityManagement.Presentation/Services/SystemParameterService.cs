using System.Collections.Immutable;
using Fluxor;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Presentation.Abstractions.Features;
using Noxy.NET.EntityManagement.Presentation.Features;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class SystemParameterService : IState<FeatureParameterSystemState>
{
    private readonly IState<FeatureParameterSystemState> _state;
    private readonly IDispatcher _dispatcher;

    public FeatureParameterSystemState Value => _state.Value;

    public event EventHandler? StateChanged;

    public SystemParameterService(IState<FeatureParameterSystemState> state, IDispatcher dispatcher)
    {
        _state = state;
        _dispatcher = dispatcher;

        _state.StateChanged += (_, _) => StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private string? GetRaw(string key, string scope)
    {
        if (Value.Collection.TryGetValue(key, out string? value)) return value;
        if (!Value.Pending.TryGetValue(scope, out ImmutableHashSet<string>? set) || !set.Contains(key))
        {
            _dispatcher.Dispatch(new ParameterReducersBase<FeatureParameterSystemState>.LoadKey(key, scope));
        }

        return null;
    }

    public bool TryGetString(string key, out string? value, string scope = "")
    {
        value = GetRaw(key, scope);
        return value != null;
    }

    public bool TryGetInt(string key, out int value, string scope = "")
    {
        value = 0;
        string? raw = GetRaw(key, scope);
        return raw != null && int.TryParse(raw, out value);
    }

    public bool TryGetDecimal(string key, out decimal value, string scope = "")
    {
        value = 0;
        string? raw = GetRaw(key, scope);
        return raw != null && decimal.TryParse(raw, out value);
    }

    public bool TryGetBoolean(string key, out bool value, string scope = "")
    {
        value = false;
        string? raw = GetRaw(key, scope);
        return raw != null && bool.TryParse(raw, out value);
    }

    public bool TryGetGuid(string key, out Guid value, string scope = "")
    {
        value = Guid.Empty;
        string? raw = GetRaw(key, scope);
        return raw != null && Guid.TryParse(raw, out value);
    }

    public bool TryGetDateTime(string key, out DateTime value, string scope = "")
    {
        value = default;
        string? raw = GetRaw(key, scope);
        return raw != null && DateTime.TryParse(raw, out value);
    }

    public bool CanPerformSchemaAction(EntitySchema schema, string inactiveKey, string deactivatedKey)
    {
        return schema.TimeActivated == null
            ? TryGetBoolean(inactiveKey, out bool canDeleteInactive) && canDeleteInactive
            : TryGetBoolean(deactivatedKey, out bool canDeleteDeactivated) && canDeleteDeactivated;
    }
}
