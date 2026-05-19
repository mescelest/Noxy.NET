using Fluxor;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Requests.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;
using Noxy.NET.EntityManagement.Presentation.Abstractions.Features;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Presentation.Features;

[FeatureState]
public record FeatureTextState : BaseFeatureParameterState;

public class FeatureTextReducers : ParameterReducersBase<FeatureTextState>;

public class FeatureTextEffects(IState<FeatureTextState> state, APIHttpClient http, IDebouncerService debouncer) : ParameterEffectsBase<FeatureTextState>(state, debouncer)
{
    protected override async Task<Dictionary<string, string?>> Resolve(IReadOnlyCollection<string> keys)
    {
        RequestDataParameterTextResolveList request = new() { SchemaIdentifierList = keys };
        ResponseDataParameterTextResolveList response = await http.SendRequest(request);
        return response.Value;
    }
}
