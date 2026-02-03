using System.Collections;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Noxy.NET.EntityManagement.Presentation.Services;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions;

public abstract class BaseHttpClient(HttpClient http, UserAuthenticationStateProvider serviceAuthentication)
{
    private static JsonSerializerOptions WriteSerializerOptions { get; } = new() { IgnoreReadOnlyProperties = true };
    private static JsonSerializerOptions QueryStringSerializerOptions { get; } = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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
