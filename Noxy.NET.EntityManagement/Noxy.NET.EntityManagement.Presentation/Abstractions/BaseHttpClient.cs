using System.Net.Http.Json;
using System.Text.Json;
using Noxy.NET.EntityManagement.Presentation.Services;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions;

public abstract class BaseHttpClient(HttpClient http, UserAuthenticationStateProvider serviceAuthentication)
{
    protected readonly JsonSerializerOptions WriteSerializerOptions = new() { IgnoreReadOnlyProperties = true };

    protected async Task<HttpResponseMessage> SendRequest(BaseFormAPIModel model)
    {
        HttpRequestMessage request = CreateRequest(model.HttpMethod, model.APIEndpoint, model);
        request.Headers.Authorization = new("Bearer", serviceAuthentication.Identity?.RawData);

        return await SendRequest(request);
    }

    protected async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request)
    {
        request.Headers.Authorization = new("Bearer", serviceAuthentication.Identity?.RawData);
        return await http.SendAsync(request);
    }

    protected HttpRequestMessage CreateRequest(HttpMethod method, string url, object? content = null)
    {
        return new()
        {
            Method = method,
            RequestUri = new(http.BaseAddress + url),
            Content = content != null ? JsonContent.Create(content, new("application/json"), WriteSerializerOptions) : null
        };
    }

    protected static HttpContent ExtractResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return response.Content;
        throw new();
    }
}
