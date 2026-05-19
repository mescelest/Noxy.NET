using Fluxor;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Requests.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;
using Noxy.NET.EntityManagement.Presentation.Abstractions.Features;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Presentation.Features;

[FeatureState]
public record FeatureSystemState : BaseFeatureParameterState;

public class FeatureSystemReducers : ParameterReducersBase<FeatureSystemState>;

public class FeatureSystemEffects(IState<FeatureSystemState> state, APIHttpClient http, IDebouncerService debouncer) : ParameterEffectsBase<FeatureSystemState>(state, debouncer)
{
    protected override async Task<Dictionary<string, string?>> Resolve(IReadOnlyCollection<string> keys)
    {
        RequestDataParameterSystemResolveList request = new() { SchemaIdentifierList = keys };
        ResponseDataParameterSystemResolveList response = await http.SendRequest(request);
        return response.Value;
    }
}
