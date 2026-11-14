using Microsoft.Net.Http.Headers;
using System.Buffers.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Noxy.NET.MediaPlayer.API;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.Test.MediaPlayer.Api;

public sealed class MediaApiClient(ApiFactory factory)
{
    private const string PlaylistsPath = "/Playlists";

    private readonly HttpClient _client = factory.CreateClient();

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

    public async Task<ApiAccount> SignUp(string email)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/Auth/SignUp", new { email, password = "a-long-enough-password" }, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        ResponseAuthentication authentication = (await response.Content.ReadFromJsonAsync<ResponseAuthentication>(TestContext.Current.CancellationToken))!;

        return new(authentication.AccessToken, ReadSubject(authentication.AccessToken));
    }

    // The title field goes first, because the server reads the body as a stream.
    public async Task<HttpResponseMessage> Upload(string? token, byte[] content, string fileName = "song.mp3", string? title = null)
    {
        return await Send(token, new ByteArrayContent(content), fileName, title);
    }

    private async Task<HttpResponseMessage> Send(string? token, HttpContent file, string fileName, string? title)
    {
        using MultipartFormDataContent form = [];
        if (title is not null) form.Add(new StringContent(title), Constants.UploadTitleField);
        form.Add(file, Constants.UploadFileField, fileName);

        using HttpRequestMessage request = new(HttpMethod.Post, "/Media");
        request.Content = form;
        Authorize(request, token);
        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    // A well-formed multipart request that has a title but no file.
    public async Task<HttpResponseMessage> UploadWithoutFile(string? token, string title)
    {
        using MultipartFormDataContent form = [];
        form.Add(new StringContent(title), Constants.UploadTitleField);

        using HttpRequestMessage request = new(HttpMethod.Post, "/Media");
        request.Content = form;
        Authorize(request, token);
        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public async Task<List<ResponseMediaFile>> GetMediaFileList(string token)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "/Media");
        Authorize(request, token);
        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<List<ResponseMediaFile>>(TestContext.Current.CancellationToken))!;
    }

    // Waits for the worker to finish the uploads, since an upload is only accepted when the request returns.
    public async Task<List<ResponseMediaFile>> WaitForMediaFileList(string token, int count)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(15);
        while (true)
        {
            List<ResponseMediaFile> list = await GetMediaFileList(token);
            if (list.Count >= count) return list;

            Assert.True(DateTime.UtcNow < deadline, "The library did not fill up in time.");
            await Task.Delay(25, TestContext.Current.CancellationToken);
        }
    }

    public async Task<HttpResponseMessage> GetLibrary(string? token, string? ifNoneMatch = null, string? acceptEncoding = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "/Media/Library");
        Authorize(request, token);
        if (ifNoneMatch is not null) request.Headers.TryAddWithoutValidation(HeaderNames.IfNoneMatch, ifNoneMatch);
        if (acceptEncoding is not null) request.Headers.TryAddWithoutValidation(HeaderNames.AcceptEncoding, acceptEncoding);
        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public async Task<HttpResponseMessage> Patch(string? token, Guid id, string json)
    {
        using HttpRequestMessage request = new(HttpMethod.Patch, $"/Media/{id}");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        Authorize(request, token);
        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public async Task<HttpResponseMessage> Delete(string? token, Guid id)
    {
        using HttpRequestMessage request = new(HttpMethod.Delete, $"/Media/{id}");
        Authorize(request, token);
        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public async Task<HttpResponseMessage> Download(string token, Guid id)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"/Media/{id}/File");
        Authorize(request, token);
        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public async Task<HttpResponseMessage> GetPlaylistLibrary(string? token, string? ifNoneMatch = null, string? acceptEncoding = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, PlaylistsPath);
        Authorize(request, token);
        if (ifNoneMatch is not null) request.Headers.TryAddWithoutValidation(HeaderNames.IfNoneMatch, ifNoneMatch);
        if (acceptEncoding is not null) request.Headers.TryAddWithoutValidation(HeaderNames.AcceptEncoding, acceptEncoding);
        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public async Task<HttpResponseMessage> GetPlaylist(string? token, Guid id, string? ifNoneMatch = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"{PlaylistsPath}/{id}");
        Authorize(request, token);
        if (ifNoneMatch is not null) request.Headers.TryAddWithoutValidation(HeaderNames.IfNoneMatch, ifNoneMatch);
        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public Task<HttpResponseMessage> CreatePlaylist(string? token, string json)
    {
        return SendJson(HttpMethod.Post, PlaylistsPath, token, json);
    }

    public Task<HttpResponseMessage> PatchPlaylist(string? token, Guid id, string json)
    {
        return SendJson(HttpMethod.Patch, $"{PlaylistsPath}/{id}", token, json);
    }

    public async Task<HttpResponseMessage> DeletePlaylist(string? token, Guid id)
    {
        using HttpRequestMessage request = new(HttpMethod.Delete, $"{PlaylistsPath}/{id}");
        Authorize(request, token);
        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public Task<HttpResponseMessage> AddPlaylistEntryList(string? token, Guid id, string json)
    {
        return SendJson(HttpMethod.Post, $"{PlaylistsPath}/{id}/Entries", token, json);
    }

    public Task<HttpResponseMessage> RemovePlaylistEntryList(string? token, Guid id, string json)
    {
        return SendJson(HttpMethod.Post, $"{PlaylistsPath}/{id}/Entries/Remove", token, json);
    }

    public Task<HttpResponseMessage> MovePlaylistEntry(string? token, Guid id, Guid entryID, string json)
    {
        return SendJson(HttpMethod.Put, $"{PlaylistsPath}/{id}/Entries/{entryID}/Position", token, json);
    }

    private async Task<HttpResponseMessage> SendJson(HttpMethod method, string path, string? token, string json)
    {
        using HttpRequestMessage request = new(method, path);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        Authorize(request, token);
        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static void Authorize(HttpRequestMessage request, string? token)
    {
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
    }

    private static Guid ReadSubject(string token)
    {
        string payload = token.Split('.')[1];
        using JsonDocument document = JsonDocument.Parse(Base64Url.DecodeFromChars(payload));

        return document.RootElement.GetProperty(JwtRegisteredClaimNames.Sub).GetGuid();
    }
}
