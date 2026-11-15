using System.Net.Http.Json;

namespace Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;

public static class HttpResponseMessageExtension
{
    extension(HttpResponseMessage response)
    {
        public string GetETag()
        {
            return response.Headers.ETag?.Tag ?? throw new InvalidOperationException("The response has no ETag header.");
        }

        public Uri GetLocation()
        {
            return response.Headers.Location ?? throw new InvalidOperationException("The response has no Location header.");
        }

        public async Task<T> ReadJson<T>()
        {
            return await response.Content.ReadFromJsonAsync<T>(TestContext.Current.CancellationToken) ?? throw new InvalidOperationException($"The response body is not a {typeof(T).Name}.");
        }
    }
}
