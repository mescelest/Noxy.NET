using Fluxor;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;
using Noxy.NET.EntityManagement.Presentation.Abstractions.Features;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Presentation.Features;

[FeatureState]
public record FeatureStyleState : BaseFeatureParameterState;

public class FeatureStyleReducers : ParameterReducersBase<FeatureStyleState>;

public class FeatureStyleEffects(IState<FeatureStyleState> state, APIHttpClient http, IDebouncerService debouncer) : ParameterEffectsBase<FeatureStyleState>(state, debouncer)
{
    protected override async Task<Dictionary<string, string?>> Resolve(IReadOnlyCollection<string> keys)
    {
        RequestDataParameterStyleResolveList request = new() { SchemaIdentifierList = keys };
        ResponseDataParameterStyleResolveList response = await http.SendRequest(request);
        return response.Value;
    }
}
