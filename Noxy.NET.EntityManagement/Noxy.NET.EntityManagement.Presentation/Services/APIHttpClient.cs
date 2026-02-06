using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class APIHttpClient(HttpClient client)
{
    private static JsonSerializerOptions WriteSerializerOptions { get; } = new() { IgnoreReadOnlyProperties = true };

    public void SetBearerToken(string token)
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
    }

    public void ClearBearerToken()
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<TResponse> SendRequest<TResponse>(BaseRequest<TResponse> request)
    {
        return request switch
        {
            BaseRequestGet<TResponse> get => await SendRequest(get),
            BaseRequestPost<TResponse> post => await SendRequest(post),
            _ => throw new NotSupportedException($"Unsupported request type: {request.GetType().Name}")
        };
    }

    public async Task<TResponse> SendRequest<TResponse>(BaseRequestGet<TResponse> request, CancellationToken cancellationToken = default)
    {
        string url = BuildUrl(request.APIEndpoint, request.ToQueryParameters());

        HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
        await EnsureSuccess(response);

        return await DeserializeOrThrow<TResponse>(response, cancellationToken);
    }

    public async Task<TResponse> SendRequest<TResponse>(BaseRequestPost<TResponse> request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(request.APIEndpoint, request.ToBody(), WriteSerializerOptions, cancellationToken);
        await EnsureSuccess(response);

        return typeof(TResponse) != typeof(Unit)
            ? await DeserializeOrThrow<TResponse>(response, cancellationToken)
            : (TResponse)(object)Unit.Value;
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        string content = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {content}");
    }

    private static async Task<T> DeserializeOrThrow<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        T? result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("Empty or invalid JSON response");
    }

    private static string BuildUrl(string baseUrl, IDictionary<string, object?> parameters)
    {
        string query = BuildQuery(parameters);
        return string.IsNullOrWhiteSpace(query) ? baseUrl : $"{baseUrl}?{query}";
    }

    private static string BuildQuery(IDictionary<string, object?> parameters)
    {
        return string.Join("&", parameters.SelectMany(p => p.Value is not null ? FormatParameter(p.Key, p.Value) : []));
    }

    private static List<string> FormatParameter(string key, object value)
    {
        return value switch
        {
            string s => [$"{key}={Uri.EscapeDataString(s)}"],
            IEnumerable<object?> seq => seq.OfType<object>().Select(item => $"{key}={Uri.EscapeDataString(FormatValue(item))}").ToList(),
            _ => [$"{key}={Uri.EscapeDataString(FormatValue(value))}"]
        };
    }

    private static string FormatValue(object v)
    {
        return v switch
        {
            bool b => b ? "true" : "false",
            DateTime dt => dt.ToString("o"),
            DateTimeOffset dto => dto.ToString("o"),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => v.ToString() ?? string.Empty
        };
    }
}
