using System.Text.Json;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class HttpResponseMessageExtension
{
    private const string TitleProperty = "title";

    extension(HttpResponseMessage response)
    {
        // The status and the problem details title the server sent. Anything else in the body is never put in the exception: it could hold a page or a token.
        public async Task<ApiException> ToApiException(CancellationToken cancellationToken = default)
        {
            string? title = null;
            try
            {
                await using Stream body = await response.Content.ReadAsStreamAsync(cancellationToken);
                using JsonDocument problem = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken);
                if (problem.RootElement.ValueKind == JsonValueKind.Object && problem.RootElement.TryGetProperty(TitleProperty, out JsonElement element) && element.ValueKind == JsonValueKind.String)
                {
                    title = element.GetString();
                }
            }
            catch (JsonException)
            {
                // Not problem details: the generic title below is used.
            }

            return new(response.StatusCode, string.IsNullOrWhiteSpace(title) ? $"The request failed ({(int)response.StatusCode})." : title);
        }
    }
}
