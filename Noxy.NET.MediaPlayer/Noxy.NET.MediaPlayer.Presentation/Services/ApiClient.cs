using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Application;
using Noxy.NET.MediaPlayer.Application.Requests.Media;
using Noxy.NET.MediaPlayer.Application.Requests.Metadata;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
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
    private const string DownloadSegment = "Download";
    private const string LibraryPath = "/Media/Library";
    private const string PlaylistPath = "/Playlist";
    private const string SettingPath = "/Settings";
    private const string ArtistPath = "/Artist";
    private const string AlbumPath = "/Album";
    private const string GenrePath = "/Genre";
    private const string WeightSegment = "Weight";
    private const string PlayedSegment = "Played";
    private const string ArtSegment = "Art";
    private const string ValueProperty = "value";
    private const string MediaFileListSegment = "MediaFileList";
    private const string RemoveSegment = "Remove";
    private const string MergeSegment = "Merge";

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

    public async Task<ApiFileResponse> GetTaggedFile(Guid id)
    {
        HttpResponseMessage response = await Send(HttpMethod.Get, $"{MediaPath}/{id}/{DownloadSegment}", null, null, null, HttpCompletionOption.ResponseHeadersRead);

        try
        {
            await EnsureSuccess(response);

            return new(response.StatusCode, await response.Content.ReadAsStreamAsync(), response.Headers.ETag?.Tag, response.Content.Headers.ContentLength, response);
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

    public async Task<ResponseMediaFile> UpdateMedia(Guid id, RequestUpdateMedia request)
    {
        return await Write<ResponseMediaFile>(HttpMethod.Patch, $"{MediaPath}/{id}", request);
    }

    public async Task DeleteMedia(Guid id)
    {
        using HttpResponseMessage response = await Send(HttpMethod.Delete, $"{MediaPath}/{id}", null, null);
        await EnsureSuccess(response);
    }

    public async Task DeletePlaylist(Guid id)
    {
        using HttpResponseMessage response = await Send(HttpMethod.Delete, $"{PlaylistPath}/{id}", null, null);
        await EnsureSuccess(response);
    }

    public async Task<ResponseUploadAccepted> UploadMedia(string fileName, string? contentType, Func<Stream> openContent)
    {
        using HttpResponseMessage response = await Send(HttpMethod.Post, MediaPath, null, null, contentFactory: () => BuildUploadContent(fileName, contentType, openContent), timeout: Timeout.InfiniteTimeSpan);
        await EnsureSuccess(response);

        return await ReadBody<ResponseUploadAccepted>(response);
    }

    public async Task<ResponsePlaylistDetail> AddPlaylistEntryList(Guid id, RequestAddPlaylistEntry request)
    {
        return await Write<ResponsePlaylistDetail>(HttpMethod.Post, $"{PlaylistPath}/{id}/EntryList", request);
    }

    public async Task<ResponsePlaylistDetail> RemovePlaylistEntryList(Guid id, RequestRemovePlaylistEntry request)
    {
        return await Write<ResponsePlaylistDetail>(HttpMethod.Post, $"{PlaylistPath}/{id}/EntryList/Remove", request);
    }

    public async Task<List<ResponsePlaylist>> MovePlaylist(Guid id, RequestMovePlaylist request)
    {
        return await Write<List<ResponsePlaylist>>(HttpMethod.Put, $"{PlaylistPath}/{id}/Position", request);
    }

    public async Task<ResponsePlaylistDetail> MovePlaylistEntry(Guid id, Guid entryID, RequestMovePlaylistEntry request)
    {
        return await Write<ResponsePlaylistDetail>(HttpMethod.Put, $"{PlaylistPath}/{id}/Entry/{entryID}/Position", request);
    }

    public async Task<ApiResponse<List<ResponseSetting>>> GetSettingList(string? eTag)
    {
        return await Get<List<ResponseSetting>>(SettingPath, eTag);
    }

    public async Task<ResponseSetting> UpdateSetting(string key, JsonElement value)
    {
        return await Write<ResponseSetting>(HttpMethod.Patch, $"{SettingPath}/{key}", new Dictionary<string, JsonElement> { [ValueProperty] = value });
    }

    public async Task<ApiResponse<List<ResponseArtist>>> GetArtistList(string? eTag)
    {
        return await Get<List<ResponseArtist>>(ArtistPath, eTag);
    }

    public async Task<ApiResponse<List<ResponseAlbum>>> GetAlbumList(string? eTag)
    {
        return await Get<List<ResponseAlbum>>(AlbumPath, eTag);
    }

    public async Task<byte[]?> GetAlbumArt(Guid id)
    {
        using HttpResponseMessage response = await Send(HttpMethod.Get, $"{AlbumPath}/{id}/{ArtSegment}", null, null);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        await EnsureSuccess(response);
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<ApiResponse<List<ResponseGenre>>> GetGenreList(string? eTag)
    {
        return await Get<List<ResponseGenre>>(GenrePath, eTag);
    }

    public async Task<ResponseArtist> MergeArtist(Guid id, RequestMergeItem request)
    {
        return await Write<ResponseArtist>(HttpMethod.Post, $"{ArtistPath}/{id}/{MergeSegment}", request);
    }

    public async Task<ResponseAlbum> MergeAlbum(Guid id, RequestMergeItem request)
    {
        return await Write<ResponseAlbum>(HttpMethod.Post, $"{AlbumPath}/{id}/{MergeSegment}", request);
    }

    public async Task<ResponseGenre> MergeGenre(Guid id, RequestMergeItem request)
    {
        return await Write<ResponseGenre>(HttpMethod.Post, $"{GenrePath}/{id}/{MergeSegment}", request);
    }

    public async Task<ResponseArtist> CreateArtist(RequestCreateArtist request)
    {
        return await Write<ResponseArtist>(HttpMethod.Post, ArtistPath, request);
    }

    public async Task<ResponseArtist> UpdateArtist(Guid id, RequestUpdateArtist request)
    {
        return await Write<ResponseArtist>(HttpMethod.Patch, $"{ArtistPath}/{id}", request);
    }

    public async Task<ResponseArtist> SetArtistWeight(Guid id, RequestSetArtistWeight request)
    {
        return await Write<ResponseArtist>(HttpMethod.Put, $"{ArtistPath}/{id}/{WeightSegment}", request);
    }

    public async Task DeleteArtist(Guid id)
    {
        using HttpResponseMessage response = await Send(HttpMethod.Delete, $"{ArtistPath}/{id}", null, null);
        await EnsureSuccess(response);
    }

    public async Task<ResponseArtist> AddArtistMediaFileList(Guid id, RequestAddMediaFileList request)
    {
        return await Write<ResponseArtist>(HttpMethod.Post, $"{ArtistPath}/{id}/{MediaFileListSegment}", request);
    }

    public async Task<ResponseArtist> RemoveArtistMediaFileList(Guid id, RequestRemoveMediaFileList request)
    {
        return await Write<ResponseArtist>(HttpMethod.Post, $"{ArtistPath}/{id}/{MediaFileListSegment}/{RemoveSegment}", request);
    }

    public async Task<ResponseAlbum> CreateAlbum(RequestCreateAlbum request)
    {
        return await Write<ResponseAlbum>(HttpMethod.Post, AlbumPath, request);
    }

    public async Task<ResponseAlbum> UpdateAlbum(Guid id, RequestUpdateAlbum request)
    {
        return await Write<ResponseAlbum>(HttpMethod.Patch, $"{AlbumPath}/{id}", request);
    }

    public async Task DeleteAlbum(Guid id)
    {
        using HttpResponseMessage response = await Send(HttpMethod.Delete, $"{AlbumPath}/{id}", null, null);
        await EnsureSuccess(response);
    }

    public async Task<ResponseAlbum> AddAlbumMediaFileList(Guid id, RequestAddMediaFileList request)
    {
        return await Write<ResponseAlbum>(HttpMethod.Post, $"{AlbumPath}/{id}/{MediaFileListSegment}", request);
    }

    public async Task<ResponseAlbum> RemoveAlbumMediaFileList(Guid id, RequestRemoveMediaFileList request)
    {
        return await Write<ResponseAlbum>(HttpMethod.Post, $"{AlbumPath}/{id}/{MediaFileListSegment}/{RemoveSegment}", request);
    }

    public async Task<ResponseGenre> CreateGenre(RequestCreateGenre request)
    {
        return await Write<ResponseGenre>(HttpMethod.Post, GenrePath, request);
    }

    public async Task<ResponseGenre> UpdateGenre(Guid id, RequestUpdateGenre request)
    {
        return await Write<ResponseGenre>(HttpMethod.Patch, $"{GenrePath}/{id}", request);
    }

    public async Task<ResponseGenre> SetGenreWeight(Guid id, RequestSetGenreWeight request)
    {
        return await Write<ResponseGenre>(HttpMethod.Put, $"{GenrePath}/{id}/{WeightSegment}", request);
    }

    public async Task DeleteGenre(Guid id)
    {
        using HttpResponseMessage response = await Send(HttpMethod.Delete, $"{GenrePath}/{id}", null, null);
        await EnsureSuccess(response);
    }

    public async Task<ResponseGenre> AddGenreMediaFileList(Guid id, RequestAddMediaFileList request)
    {
        return await Write<ResponseGenre>(HttpMethod.Post, $"{GenrePath}/{id}/{MediaFileListSegment}", request);
    }

    public async Task<ResponseGenre> RemoveGenreMediaFileList(Guid id, RequestRemoveMediaFileList request)
    {
        return await Write<ResponseGenre>(HttpMethod.Post, $"{GenrePath}/{id}/{MediaFileListSegment}/{RemoveSegment}", request);
    }

    public async Task SetAlbumArt(Guid id, byte[] picture, string contentType)
    {
        using HttpResponseMessage response = await Send(HttpMethod.Put, $"{AlbumPath}/{id}/{ArtSegment}", null, null, contentFactory: () => BuildPictureContent(picture, contentType));
        await EnsureSuccess(response);
    }

    public async Task MarkPlaylistPlayed(Guid id)
    {
        using HttpResponseMessage response = await Send(HttpMethod.Post, $"{PlaylistPath}/{id}/{PlayedSegment}", null, null);
        await EnsureSuccess(response);
    }

    private async Task<ApiResponse<T>> Get<T>(string path, string? eTag)
    {
        using HttpResponseMessage response = await Send(HttpMethod.Get, path, null, eTag);
        if (response.StatusCode == HttpStatusCode.NotModified) return new(HttpStatusCode.NotModified, default, response.Headers.ETag?.Tag);
        if (response.StatusCode == HttpStatusCode.NoContent) return new(HttpStatusCode.NoContent, default, null);

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
    private async Task<HttpResponseMessage> Send(HttpMethod method, string path, object? body, string? eTag, FileRange? range = null, HttpCompletionOption completion = HttpCompletionOption.ResponseContentRead, Func<HttpContent>? contentFactory = null,
        TimeSpan? timeout = null)
    {
        string address = await storageServerAddress.GetAddress() ?? throw new ServerAddressMissingException();

        HttpResponseMessage response = await SendOnce(address, method, path, body, eTag, range, completion, contentFactory, timeout);
        if (response.StatusCode != HttpStatusCode.Unauthorized) return response;

        if (!await serviceSession.Refresh()) throw await ToExceptionAndDispose(response);

        response.Dispose();
        response = await SendOnce(address, method, path, body, eTag, range, completion, contentFactory, timeout);
        if (response.StatusCode == HttpStatusCode.Unauthorized) throw await ToExceptionAndDispose(response);

        return response;
    }

    private async Task<HttpResponseMessage> SendOnce(string address, HttpMethod method, string path, object? body, string? eTag, FileRange? range, HttpCompletionOption completion, Func<HttpContent>? contentFactory, TimeSpan? timeout)
    {
        using HttpRequestMessage request = new(method, $"{address}{path}");
        request.Content = contentFactory is not null ? contentFactory() : body is null ? null : JsonContent.Create(body);
        if (await serviceSession.GetAccessToken() is { } token) request.Headers.Authorization = new(BearerScheme, token);
        if (eTag is not null) request.Headers.TryAddWithoutValidation(IfNoneMatchHeader, eTag);
        if (range is not null) request.Headers.Range = new(range.Start, null);
        if (range?.IfRange is not null) request.Headers.IfRange = new(EntityTagHeaderValue.Parse(range.IfRange));

        using CancellationTokenSource timeoutSource = new(timeout ?? TimeSpan.FromSeconds(PresentationConstants.HttpTimeoutSeconds));

        return await client.SendAsync(request, completion, timeoutSource.Token);
    }

    private static ByteArrayContent BuildPictureContent(byte[] picture, string contentType)
    {
        ByteArrayContent content = new(picture);
        content.Headers.ContentType = new(contentType);

        return content;
    }

    private static MultipartFormDataContent BuildUploadContent(string fileName, string? contentType, Func<Stream> openContent)
    {
        StreamContent file = new(openContent());
        if (MediaTypeHeaderValue.TryParse(contentType, out MediaTypeHeaderValue? parsed)) file.Headers.ContentType = parsed;

        return new() { { file, ApplicationConstants.FileField, fileName } };
    }

    private sealed record FileRange(long Start, string? IfRange);

    private static async Task<ApiException> ToExceptionAndDispose(HttpResponseMessage response)
    {
        using (response)
        {
            return await response.ToApiException();
        }
    }
}
