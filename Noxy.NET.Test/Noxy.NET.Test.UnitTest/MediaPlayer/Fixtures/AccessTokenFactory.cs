using System.Text;
using System.Text.Json;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public static class AccessTokenFactory
{
    // A token that looks like the API's: three base64url segments, with an exp claim when one is given. The signature is not real, the client never checks it.
    public static string Create(DateTime? expires, string? subject = null)
    {
        Dictionary<string, object> payload = new() { ["sub"] = subject ?? "user" };
        if (expires is { } time) payload["exp"] = new DateTimeOffset(time, TimeSpan.Zero).ToUnixTimeSeconds();

        return $"{Encode("{\"alg\":\"none\"}")}.{Encode(JsonSerializer.Serialize(payload))}.{Encode("signature")}";
    }

    private static string Encode(string text)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(text)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
