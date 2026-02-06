using System.Collections;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions;

public abstract class BaseHttpClient(HttpClient http, UserAuthenticationStateProvider serviceAuthentication)
{
    private static JsonSerializerOptions WriteSerializerOptions { get; } = new() { IgnoreReadOnlyProperties = true };
    private static JsonSerializerOptions QueryStringSerializerOptions { get; } = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };


    public void SetBearerToken(string token)
    {
        http.DefaultRequestHeaders.Authorization = new("Bearer", token);
    }

    public void ClearBearerToken()
    {
        http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<TResponse> SendRequest<TResponse>(BaseRequest<TResponse> request)
    {
        return request switch { BaseRequestGet<TResponse> get => await SendRequest(get), BaseRequestPost<TResponse> post => await SendRequest(post), _ => throw new NotSupportedException($"Unsupported request type: {request.GetType().Name}") };
    }

    public async Task<TResponse> SendRequest<TResponse>(BaseRequestGet<TResponse> request, CancellationToken cancellationToken = default)
    {
        string url = BuildUrl(request.APIEndpoint, request.ToQueryParameters());

        HttpResponseMessage response = await http.GetAsync(url, cancellationToken);
        await EnsureSuccess(response);

        return await DeserializeOrThrow<TResponse>(response, cancellationToken);
    }

    public async Task<TResponse> SendRequest<TResponse>(BaseRequestPost<TResponse> request, CancellationToken cancellationToken = default)
    {
        object? body = request.ToBody();
        HttpResponseMessage response = await http.PostAsJsonAsync(request.APIEndpoint, body, cancellationToken);
        await EnsureSuccess(response);

        if (typeof(TResponse) == typeof(Unit))
            return (TResponse)(object)Unit.Value;

        return await DeserializeOrThrow<TResponse>(response, cancellationToken);
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        string content = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(
            $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {content}");
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
        return string.Join("&", parameters.SelectMany(p => p.Value is null ? [] : FormatParameter(p.Key, p.Value)));
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
        HttpRequestMessage request = new()
        {
            Method = method,
        };

        if (method == HttpMethod.Get || method == HttpMethod.Head)
        {
            if (content is not null and not IDictionary<string, object?>)
            {
                throw new ArgumentException("Content for GET/HEAD requests must be an IDictionary<string, object?> for query parameters.", nameof(content));
            }

            IDictionary<string, object?>? queryParams = content as IDictionary<string, object?>;
            string queryString = BuildQueryString(queryParams);
            string urlWithQuery = string.IsNullOrEmpty(queryString) ? url : $"{url}?{queryString}";
            request.RequestUri = new(http.BaseAddress + urlWithQuery);
        }
        else
        {
            request.RequestUri = new(http.BaseAddress + url);
            request.Content = content != null ? JsonContent.Create(content, new("application/json"), WriteSerializerOptions) : null;
        }

        return request;
    }

    public static HttpContent ExtractResponse(HttpResponseMessage response)
    {
        return response.IsSuccessStatusCode ? response.Content : throw new();
    }

    private static string BuildQueryString(IDictionary<string, object?>? parameters)
    {
        if (parameters is null) return string.Empty;

        List<string> parts = [];
        foreach (KeyValuePair<string, object?> kvp in parameters)
        {
            parts.AddRange(BuildQueryParts(kvp.Key, kvp.Value));
        }

        return parts.Count > 0 ? string.Join("&", parts) : string.Empty;
    }

    private static IEnumerable<string> BuildQueryParts(string key, object? value)
    {
        if (value is null) return [];

        if (IsSimple(value))
        {
            return [$"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(FormatSimple(value))}"];
        }

        if (value is IEnumerable list and not string)
        {
            return list.OfType<object>()
                .Select(item => IsSimple(item)
                    ? WebUtility.UrlEncode(FormatSimple(item))
                    : WebUtility.UrlEncode(JsonSerializer.Serialize(item, QueryStringSerializerOptions)))
                .Select(encodedValue => $"{WebUtility.UrlEncode(key)}={encodedValue}");
        }

        return [$"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(JsonSerializer.Serialize(value, QueryStringSerializerOptions))}"];
    }

    private static bool IsSimple(object? value)
    {
        return value is string or ValueType or DateTime or DateTimeOffset or TimeSpan or Guid;
    }

    private static string FormatSimple(object? value)
    {
        return value switch
        {
            bool b => b.ToString().ToLowerInvariant(),
            float f => f.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            decimal d => d.ToString(CultureInfo.InvariantCulture),
            DateTime dt => dt.ToString("o"),
            DateTimeOffset dto => dto.ToString("o"),
            _ => value?.ToString() ?? string.Empty
        };
    }
}
