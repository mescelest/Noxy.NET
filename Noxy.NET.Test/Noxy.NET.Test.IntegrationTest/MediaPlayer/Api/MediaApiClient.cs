using System.Buffers.Text;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.Test.MediaPlayer.Api;

public sealed class MediaApiClient(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    public record Account(string Token, Guid UserID);

    public static byte[] Mp3(int length = 1000)
    {
        byte[] bytes = new byte[length];
        "ID3"u8.CopyTo(bytes);
        for (int i = 3; i < length; i++)
        {
            bytes[i] = (byte)(i * 31);
        }

        return bytes;
    }

    public async Task<Account> SignUp(string email)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/Auth/SignUp", new { email, password = "a-long-enough-password" }, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        ResponseAuthentication authentication = (await response.Content.ReadFromJsonAsync<ResponseAuthentication>(TestContext.Current.CancellationToken))!;

        return new(authentication.AccessToken, ReadSubject(authentication.AccessToken));
    }

    // The title field goes first, because the server reads the body as a stream.
    public async Task<HttpResponseMessage> Upload(string? token, byte[] content, string fileName = "song.mp3", string? title = null)
    {
        using MultipartFormDataContent form = [];
        if (title != null) form.Add(new StringContent(title), "title");
        form.Add(new ByteArrayContent(content), "file", fileName);

        using HttpRequestMessage request = new(HttpMethod.Post, "/Media") { Content = form };
        Authorize(request, token);
        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public async Task<List<ResponseMediaFile>> List(string token)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "/Media");
        Authorize(request, token);
        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<List<ResponseMediaFile>>(TestContext.Current.CancellationToken))!;
    }

    public async Task<HttpResponseMessage> Download(string token, Guid id)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"/Media/{id}/File");
        Authorize(request, token);
        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static void Authorize(HttpRequestMessage request, string? token)
    {
        if (token != null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static Guid ReadSubject(string token)
    {
        string payload = token.Split('.')[1];
        using JsonDocument document = JsonDocument.Parse(Base64Url.DecodeFromChars(payload));

        return document.RootElement.GetProperty("sub").GetGuid();
    }
}
