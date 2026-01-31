using System.Net.Http.Json;
using Noxy.NET.EntityManagement.Presentation.Services;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions;

public abstract class BaseHttpClientForm(HttpClient http, UserAuthenticationStateProvider serviceAuthentication) : BaseHttpClient(http, serviceAuthentication)
{
    public async Task<HttpResponseMessage> SendRequest(BaseFormAPIModel model)
    {
        HttpRequestMessage request = CreateRequest(model.HttpMethod, model.APIEndpoint, model);
        return await SendRequest(request);
    }

    public async Task<T> SendRequest<T>(BaseFormAPIModel model)
    {
        HttpRequestMessage request = CreateRequest(model.HttpMethod, model.APIEndpoint, model);
        HttpContent response = ExtractResponse(await SendRequest(request));
        return await response.ReadFromJsonAsync<T>() ?? throw new FormatException();
    }

    public async Task<T> SendRequest<T>(BaseFormAPIModel<T> model)
    {
        return await SendRequest<T>(model as BaseFormAPIModel);
    }
}
