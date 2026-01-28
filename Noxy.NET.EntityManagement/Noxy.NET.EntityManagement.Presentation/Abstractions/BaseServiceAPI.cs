using System.Net.Http.Json;
using System.Text.Json;
using Noxy.NET.EntityManagement.Presentation.Services;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions;

public abstract class BaseServiceAPI(HttpClient http, UserAuthenticationStateProvider serviceAuthentication)
{
    protected readonly JsonSerializerOptions WriteSerializerOptions = new() { IgnoreReadOnlyProperties = true };

    public async Task<TResult> PostForm<TResult>(BaseFormAPIModel model)
    {
        HttpRequestMessage requestMessage = CreateRequest(model.HttpMethod, model.APIEndpoint, model);
        HttpContent response = HandleResponse(await SendRequest(requestMessage));
        return await response.ReadFromJsonAsync<TResult>() ?? throw new FormatException();
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

    protected async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request)
    {
        request.Headers.Authorization = new("Bearer", serviceAuthentication.Identity?.RawData);
        return await http.SendAsync(request);
    }

    protected static HttpContent HandleResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return response.Content;
        throw new();
    }
}
