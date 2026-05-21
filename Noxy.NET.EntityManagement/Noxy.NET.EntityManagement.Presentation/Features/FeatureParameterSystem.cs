using Fluxor;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;
using Noxy.NET.EntityManagement.Presentation.Abstractions.Features;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Presentation.Features;

[FeatureState]
public record FeatureParameterSystemState : BaseFeatureParameterState;

public class FeatureParameterSystemReducers : ParameterReducersBase<FeatureParameterSystemState>;

public class FeatureParameterSystemEffects(IState<FeatureParameterSystemState> state, APIHttpClient http, IDebouncerService debouncer) : ParameterEffectsBase<FeatureParameterSystemState>(state, debouncer)
{
    protected override async Task<Dictionary<string, string?>> Resolve(IReadOnlyCollection<string> keys)
    {
        RequestDataParameterSystemResolveList request = new() { SchemaIdentifierList = keys };
        ResponseDataParameterSystemResolveList response = await http.SendRequest(request);
        return response.Value;
    }
}
