using System.Net.Http.Json;
using System.Text.Json;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions;

public abstract class BaseHttpClient(HttpClient http, UserAuthenticationStateProvider serviceAuthentication)
{
    private JsonSerializerOptions WriteSerializerOptions { get; } = new() { IgnoreReadOnlyProperties = true };

    protected async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request)
    {
        request.Headers.Authorization = new("Bearer", serviceAuthentication.Identity?.RawData);
        return await http.SendAsync(request);
    }

    public async Task<HttpResponseMessage> SendRequest(HttpMethod method, string url, object? content = null)
    {
        return await SendRequest(CreateRequest(method, url, content));
    }

    protected async Task<T> SendRequest<T>(HttpRequestMessage request)
    {
        HttpContent response = ExtractResponse(await SendRequest(request));
        return await response.ReadFromJsonAsync<T>() ?? throw new FormatException();
    }

    public async Task<T> SendRequest<T>(HttpMethod method, string url, object? content = null)
    {
        return await SendRequest<T>(CreateRequest(method, url, content));
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

    public static HttpContent ExtractResponse(HttpResponseMessage response)
    {
        return response.IsSuccessStatusCode ? response.Content : throw new();
    }
}
