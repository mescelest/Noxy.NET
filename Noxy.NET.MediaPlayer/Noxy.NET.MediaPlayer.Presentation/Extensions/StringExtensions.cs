using System.Buffers.Text;
using System.Text.Json;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class StringExtensions
{
    private const string ExpiresClaim = "exp";

    extension(string accessToken)
    {
        // The exp claim of the token's payload as a UTC time, read without validating the token, which is the server's job. Null when the token cannot be read or has no exp claim.
        public DateTime? GetAccessTokenExpires()
        {
            string[] segmentList = accessToken.Split('.');
            if (segmentList.Length != 3) return null;

            try
            {
                using JsonDocument payload = JsonDocument.Parse(Base64Url.DecodeFromChars(segmentList[1]));
                if (payload.RootElement.ValueKind != JsonValueKind.Object || !payload.RootElement.TryGetProperty(ExpiresClaim, out JsonElement expires) || !expires.TryGetInt64(out long seconds)) return null;

                return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
            }
            catch (Exception exception) when (exception is FormatException or JsonException or ArgumentOutOfRangeException)
            {
                return null;
            }
        }
    }

    extension(string address)
    {
        // An absolute http or https address, scheme and host in lower case, without a trailing slash. Throws ArgumentException for anything else, including an address with a query, a fragment or credentials.
        public string NormalizeServerAddress()
        {
            if (!Uri.TryCreate(address.Trim(), UriKind.Absolute, out Uri? uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) || uri.Query.Length > 0 || uri.Fragment.Length > 0 || uri.UserInfo.Length > 0)
            {
                throw new ArgumentException("The server address must be an http or https address.", nameof(address));
            }

            return uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
        }
    }
}
