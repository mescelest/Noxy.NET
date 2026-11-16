using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class ApiClient(HttpClient client, ISessionService serviceSession, IServerAddressStorage storageServerAddress) : IApiClient
{
    private const string BearerScheme = "Bearer";
    private const string IfNoneMatchHeader = "If-None-Match";
    private const string MediaPath = "/Media";
    private const string FileSegment = "File";
    private const string LibraryPath = "/Media/Library";
    private const string PlaylistPath = "/Playlist";

    public async Task<ApiResponse<List<ResponseMediaFile>>> GetLibrary(string? eTag)
    {
        return await Get<List<ResponseMediaFile>>(LibraryPath, eTag);
    }

    public async Task<ApiResponse<List<ResponsePlaylist>>> GetPlaylistList(string? eTag)
    {
        return await Get<List<ResponsePlaylist>>(PlaylistPath, eTag);
    }

    public async Task<ApiResponse<ResponsePlaylistDetail>> GetPlaylistDetail(Guid id, string? eTag)
    {
        return await Get<ResponsePlaylistDetail>($"{PlaylistPath}/{id}", eTag);
    }

    public async Task<ApiFileResponse> GetFile(Guid id, long? rangeStart, string? ifRange)
    {
        FileRange? range = rangeStart is null ? null : new(rangeStart.Value, ifRange);
        HttpResponseMessage response = await Send(HttpMethod.Get, $"{MediaPath}/{id}/{FileSegment}", null, null, range, HttpCompletionOption.ResponseHeadersRead);

        try
        {
            await EnsureSuccess(response);
            Stream content = await response.Content.ReadAsStreamAsync();
            long? total = response.StatusCode == HttpStatusCode.PartialContent ? response.Content.Headers.ContentRange?.Length : response.Content.Headers.ContentLength;

            return new(response.StatusCode, content, response.Headers.ETag?.Tag, total, response);
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    public async Task<ResponsePlaylist> CreatePlaylist(RequestCreatePlaylist request)
    {
        return await Write<ResponsePlaylist>(HttpMethod.Post, PlaylistPath, request);
    }

    public async Task<ResponsePlaylist> UpdatePlaylist(Guid id, RequestUpdatePlaylist request)
    {
        return await Write<ResponsePlaylist>(HttpMethod.Patch, $"{PlaylistPath}/{id}", request);
    }

    public async Task DeletePlaylist(Guid id)
    {
        using HttpResponseMessage response = await Send(HttpMethod.Delete, $"{PlaylistPath}/{id}", null, null);
        await EnsureSuccess(response);
    }

    public async Task<ResponsePlaylistDetail> AddPlaylistEntryList(Guid id, RequestAddPlaylistEntry request)
    {
        return await Write<ResponsePlaylistDetail>(HttpMethod.Post, $"{PlaylistPath}/{id}/EntryList", request);
    }

    public async Task<ResponsePlaylistDetail> RemovePlaylistEntryList(Guid id, RequestRemovePlaylistEntry request)
    {
        return await Write<ResponsePlaylistDetail>(HttpMethod.Post, $"{PlaylistPath}/{id}/EntryList/Remove", request);
    }

    public async Task<ResponsePlaylistDetail> MovePlaylistEntry(Guid id, Guid entryID, RequestMovePlaylistEntry request)
    {
        return await Write<ResponsePlaylistDetail>(HttpMethod.Put, $"{PlaylistPath}/{id}/Entry/{entryID}/Position", request);
    }

    private async Task<ApiResponse<T>> Get<T>(string path, string? eTag)
    {
        using HttpResponseMessage response = await Send(HttpMethod.Get, path, null, eTag);
        if (response.StatusCode == HttpStatusCode.NotModified) return new(HttpStatusCode.NotModified, default, response.Headers.ETag?.Tag);

        await EnsureSuccess(response);
        T value = await ReadBody<T>(response);
        return new(response.StatusCode, value, response.Headers.ETag?.Tag);
    }

    private async Task<T> Write<T>(HttpMethod method, string path, object body)
    {
        using HttpResponseMessage response = await Send(method, path, body, null);
        await EnsureSuccess(response);

        return await ReadBody<T>(response);
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode) throw await response.ToApiException();
    }

    private static async Task<T> ReadBody<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>() ?? throw new ApiException(HttpStatusCode.BadGateway, "The server sent an answer the app cannot read.");
    }

    // Sends the request with the current access token. An unauthorized answer gets one refresh and one more try with a fresh copy of the request (the body is built again for each try, since sending disposes it); if the refresh fails or the second try is unauthorized as well, the answer is thrown as it came.
    private async Task<HttpResponseMessage> Send(HttpMethod method, string path, object? body, string? eTag, FileRange? range = null, HttpCompletionOption completion = HttpCompletionOption.ResponseContentRead)
    {
        string address = await storageServerAddress.GetAddress() ?? throw new ServerAddressMissingException();

        HttpResponseMessage response = await SendOnce(address, method, path, body, eTag, range, completion);
        if (response.StatusCode != HttpStatusCode.Unauthorized) return response;

        if (!await serviceSession.Refresh()) throw await ToExceptionAndDispose(response);

        response.Dispose();
        response = await SendOnce(address, method, path, body, eTag, range, completion);
        if (response.StatusCode == HttpStatusCode.Unauthorized) throw await ToExceptionAndDispose(response);

        return response;
    }

    private async Task<HttpResponseMessage> SendOnce(string address, HttpMethod method, string path, object? body, string? eTag, FileRange? range, HttpCompletionOption completion)
    {
        using HttpRequestMessage request = new(method, $"{address}{path}");
        request.Content = body is null ? null : JsonContent.Create(body);
        if (await serviceSession.GetAccessToken() is { } token) request.Headers.Authorization = new(BearerScheme, token);
        if (eTag is not null) request.Headers.TryAddWithoutValidation(IfNoneMatchHeader, eTag);
        if (range is not null) request.Headers.Range = new(range.Start, null);
        if (range?.IfRange is not null) request.Headers.IfRange = new(EntityTagHeaderValue.Parse(range.IfRange));

        return await client.SendAsync(request, completion);
    }

    // Where a ranged download starts, and the ETag the bytes already held came with.
    private sealed record FileRange(long Start, string? IfRange);

    private static async Task<ApiException> ToExceptionAndDispose(HttpResponseMessage response)
    {
        using (response)
        {
            return await response.ToApiException();
        }
    }
}
