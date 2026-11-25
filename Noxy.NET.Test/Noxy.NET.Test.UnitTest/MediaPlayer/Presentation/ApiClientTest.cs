using System.Net;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class ApiClientTest
{
    private static readonly Guid PlaylistID = Guid.CreateVersion7();
    private static readonly Guid EntryID = Guid.CreateVersion7();

    private readonly FakeHttpMessageHandler _handler = new();
    private readonly FakeSessionService _session = new();
    private readonly FakeServerAddressStorage _address = new();
    private readonly ApiClient _client;

    public ApiClientTest()
    {
        _client = new(new(_handler), _session, _address);
    }

    private static ResponseMediaFile MediaFile(string title = "Media")
    {
        return new()
        {
            ID = Guid.CreateVersion7(),
            Title = title,
            ContentType = "audio/mpeg",
            SizeBytes = 3,
            ContentHash = new('a', 64),
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };
    }

    private static ResponsePlaylist Playlist()
    {
        return new()
        {
            ID = PlaylistID,
            Name = "Mix",
            AllowDuplicates = true,
            EntryCount = 0,
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow,
            TimeLastPlayed = null
        };
    }

    private static ResponsePlaylistDetail Detail()
    {
        return new()
        {
            ID = PlaylistID,
            Name = "Mix",
            AllowDuplicates = true,
            EntryCount = 0,
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow,
            TimeLastPlayed = null,
            EntryList = []
        };
    }

    private static HttpResponseMessage Ok(object value, string? eTag = null)
    {
        HttpResponseMessage response = FakeHttpMessageHandler.Json(HttpStatusCode.OK, JsonSerializer.Serialize(value, JsonSerializerOptions.Web));
        if (eTag is not null) response.Headers.TryAddWithoutValidation("ETag", eTag);

        return response;
    }

    private FakeHttpMessageHandler.RecordedRequest Single()
    {
        return Assert.Single(_handler.RequestList);
    }

    #region -- Reads --

    [Fact]
    public async Task GetLibrary_SendsTheBearerTokenAndTheETagAsIfNoneMatch_ToTheSavedAddress()
    {
        _handler.Respond = _ => Ok(new List<ResponseMediaFile>());

        await _client.GetLibrary("\"abc\"");

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("https://server.example/Media/Library", request.Uri?.ToString());
        Assert.Equal("Bearer token-1", request.Authorization);
        Assert.Equal("\"abc\"", request.IfNoneMatch);
    }

    [Fact]
    public async Task GetLibrary_On200_ReturnsTheListAndTheETagHeader()
    {
        ResponseMediaFile file = MediaFile("Song title");
        _handler.Respond = _ => Ok(new List<ResponseMediaFile> { file }, "\"etag-1\"");

        ApiResponse<List<ResponseMediaFile>> response = await _client.GetLibrary(null);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.False(response.IsNotModified);
        Assert.Equal("\"etag-1\"", response.ETag);
        ResponseMediaFile read = Assert.Single(response.Value ?? []);
        Assert.Equal(file.ID, read.ID);
        Assert.Equal("Song title", read.Title);
    }

    [Fact]
    public async Task GetLibrary_On304_ReturnsNotModifiedWithNoValue()
    {
        _handler.Respond = _ => new(HttpStatusCode.NotModified);

        ApiResponse<List<ResponseMediaFile>> response = await _client.GetLibrary("\"etag-1\"");

        Assert.True(response.IsNotModified);
        Assert.Null(response.Value);
    }

    [Fact]
    public async Task GetLibrary_WithNoETag_SendsNoIfNoneMatch()
    {
        _handler.Respond = _ => Ok(new List<ResponseMediaFile>());

        await _client.GetLibrary(null);

        Assert.Null(Single().IfNoneMatch);
    }

    [Fact]
    public async Task GetPlaylistList_AndGetPlaylistDetail_UseTheirPathsAndReturnTheirETags()
    {
        _handler.Respond = request => request.Uri?.AbsolutePath == "/Playlist" ? Ok(new List<ResponsePlaylist> { Playlist() }, "\"list\"") : Ok(Detail(), "\"detail\"");

        ApiResponse<List<ResponsePlaylist>> list = await _client.GetPlaylistList("\"old\"");
        ApiResponse<ResponsePlaylistDetail> detail = await _client.GetPlaylistDetail(PlaylistID, null);

        Assert.Equal("\"list\"", list.ETag);
        Assert.Single(list.Value ?? []);
        Assert.Equal("\"detail\"", detail.ETag);
        Assert.Equal(PlaylistID, detail.Value?.ID);
        Assert.Equal(["https://server.example/Playlist", $"https://server.example/Playlist/{PlaylistID}"], _handler.RequestList.Select(x => x.Uri?.ToString()));
        Assert.Equal("\"old\"", _handler.RequestList[0].IfNoneMatch);
    }

    #endregion -- Reads --

    #region -- Address and failures --

    [Fact]
    public async Task AnyCall_UsesTheSavedServerAddress_AndANewAddressAppliesToTheNextCall()
    {
        _handler.Respond = _ => Ok(new List<ResponseMediaFile>());

        await _client.GetLibrary(null);
        await _address.SetAddress("http://other.example:8080/");
        await _client.GetLibrary(null);

        Assert.Equal(["https://server.example/Media/Library", "http://other.example:8080/Media/Library"], _handler.RequestList.Select(x => x.Uri?.ToString()));
    }

    [Fact]
    public async Task AnyCall_WithNoSavedAddress_ThrowsServerAddressMissingException_AndSendsNothing()
    {
        _address.Address = null;

        await Assert.ThrowsAsync<ServerAddressMissingException>(() => _client.GetLibrary(null));
        await Assert.ThrowsAsync<ServerAddressMissingException>(() => _client.DeletePlaylist(PlaylistID));

        Assert.Empty(_handler.RequestList);
    }

    [Fact]
    public async Task ANetworkFailure_PropagatesAsHttpRequestException()
    {
        _handler.Failure = new HttpRequestException("The network is down.");

        await Assert.ThrowsAsync<HttpRequestException>(() => _client.GetLibrary(null));
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "The playlist was not found.")]
    [InlineData(HttpStatusCode.Conflict, "The playlist is full.")]
    [InlineData(HttpStatusCode.BadRequest, "The position is outside the playlist.")]
    public async Task AProblemResponse_ThrowsApiExceptionWithItsStatusAndTitle(HttpStatusCode status, string title)
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(status, title);

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _client.GetPlaylistDetail(PlaylistID, null));

        Assert.Equal(status, exception.Status);
        Assert.Equal(title, exception.Title);
    }

    [Fact]
    public async Task AReadWhoseBodyIsNotTheExpectedShape_ThrowsApiException()
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Json(HttpStatusCode.OK, "null");

        await Assert.ThrowsAsync<ApiException>(() => _client.GetLibrary(null));
    }

    #endregion -- Address and failures --

    #region -- Refresh on 401 --

    [Fact]
    public async Task AnUnauthorizedResponse_RefreshesOnceAndRetriesOnce_WithTheNewToken()
    {
        _handler.Respond = request => request.Authorization == "Bearer token-2" ? Ok(new List<ResponseMediaFile>()) : FakeHttpMessageHandler.Problem(HttpStatusCode.Unauthorized, "Unauthorized.");

        ApiResponse<List<ResponseMediaFile>> response = await _client.GetLibrary(null);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(1, _session.RefreshCount);
        Assert.Equal(["Bearer token-1", "Bearer token-2"], _handler.RequestList.Select(x => x.Authorization));
    }

    [Fact]
    public async Task AnUnauthorizedResponse_AfterTheRefreshFailed_ThrowsApiException401_AndDoesNotRetry()
    {
        _session.RefreshSucceeds = false;
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(HttpStatusCode.Unauthorized, "Invalid credentials.");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _client.GetLibrary(null));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.Status);
        Assert.Equal("Invalid credentials.", exception.Title);
        Assert.Single(_handler.RequestList);
        Assert.Equal(1, _session.RefreshCount);
    }

    [Fact]
    public async Task AnUnauthorizedResponse_ThatStaysUnauthorizedAfterTheRetry_ThrowsApiException401_WithoutRefreshingAgain()
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(HttpStatusCode.Unauthorized, "Unauthorized.");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _client.GetLibrary(null));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.Status);
        Assert.Equal(2, _handler.RequestList.Count);
        Assert.Equal(1, _session.RefreshCount);
    }

    [Fact]
    public async Task WhenSignedOut_TheCallGoesWithoutABearerToken_AndAnUnauthorizedAnswerIsNotRetriedIfTheRefreshFails()
    {
        _session.AccessToken = null;
        _session.RefreshSucceeds = false;
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(HttpStatusCode.Unauthorized, "Unauthorized.");

        await Assert.ThrowsAsync<ApiException>(() => _client.GetLibrary(null));

        Assert.Null(Single().Authorization);
    }

    [Fact]
    public async Task ConcurrentUnauthorizedResponses_RefreshOnce_WithTheRealSessionService()
    {
        FakeTimeProvider time = new();
        FakeAuthApiClient clientAuthApi = new(time);
        FakeSessionStorage storageSession = new();
        SessionService session = new(clientAuthApi, new FakeGoogleSignInService(), storageSession, time);
        await session.SignIn("a@example.com", "pw");
        ApiClient client = new(new(_handler), session, _address);
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        clientAuthApi.RefreshGate = gate.Task;
        _handler.Respond = request => request.Authorization == "Bearer " + RefreshedToken(clientAuthApi) ? Ok(new List<ResponseMediaFile>()) : FakeHttpMessageHandler.Problem(HttpStatusCode.Unauthorized, "Unauthorized.");

        List<Task<ApiResponse<List<ResponseMediaFile>>>> callList = [.. Enumerable.Range(0, 5).Select(_ => client.GetLibrary(null))];
        await Task.Delay(100, TestContext.Current.CancellationToken);
        gate.SetResult();
        ApiResponse<List<ResponseMediaFile>>[] responseList = await Task.WhenAll(callList);

        Assert.All(responseList, x => Assert.Equal(HttpStatusCode.OK, x.Status));
        Assert.Equal(1, clientAuthApi.RefreshCount);
    }

    // The access token the fake auth client hands out on its next call, which is the one a refresh in this test produces.
    private static string RefreshedToken(FakeAuthApiClient clientAuthApi)
    {
        return clientAuthApi.RefreshCount == 0 ? string.Empty : clientAuthApi.LastAccessToken ?? string.Empty;
    }

    #endregion -- Refresh on 401 --

    #region -- Writes --

    [Fact]
    public async Task CreatePlaylist_PostsTheBody_AndReturnsThePlaylist()
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Json(HttpStatusCode.Created, JsonSerializer.Serialize(Playlist(), JsonSerializerOptions.Web));

        ResponsePlaylist created = await _client.CreatePlaylist(new() { Name = "Mix", AllowDuplicates = false });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://server.example/Playlist", request.Uri?.ToString());
        Assert.Equal("Mix", JsonDocument.Parse(request.Body ?? "{}").RootElement.GetProperty("name").GetString());
        Assert.False(JsonDocument.Parse(request.Body ?? "{}").RootElement.GetProperty("allowDuplicates").GetBoolean());
        Assert.Equal(PlaylistID, created.ID);
    }

    [Fact]
    public async Task UpdatePlaylist_PatchesThePlaylist()
    {
        _handler.Respond = _ => Ok(Playlist());

        await _client.UpdatePlaylist(PlaylistID, new() { Name = new("Renamed") });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal($"https://server.example/Playlist/{PlaylistID}", request.Uri?.ToString());
        Assert.Equal("Renamed", JsonDocument.Parse(request.Body ?? "{}").RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task UpdatePlaylist_SendsOnlyTheFieldsThatWereSet_SoALeftOutFieldIsNotChanged()
    {
        _handler.Respond = _ => Ok(Playlist());

        await _client.UpdatePlaylist(PlaylistID, new() { Name = new("Renamed") });

        Assert.Equal("""{"name":"Renamed"}""", Single().Body);
    }

    [Fact]
    public async Task MovePlaylist_PutsThePosition_AndReturnsTheListInTheNewOrder()
    {
        _handler.Respond = _ => Ok(new List<ResponsePlaylist> { Playlist() });

        List<ResponsePlaylist> result = await _client.MovePlaylist(PlaylistID, new() { Position = 2 });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal($"https://server.example/Playlist/{PlaylistID}/Position", request.Uri?.ToString());
        Assert.Equal("""{"position":2}""", request.Body);
        Assert.Single(result);
    }

    [Fact]
    public async Task UpdateMedia_PatchesTheMediaFile_AndReturnsItAsItIsNow()
    {
        ResponseMediaFile media = MediaFile("New");
        _handler.Respond = _ => Ok(media);

        ResponseMediaFile result = await _client.UpdateMedia(media.ID, new() { Title = new("New") });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal($"https://server.example/Media/{media.ID}", request.Uri?.ToString());
        Assert.Equal("New", result.Title);
    }

    [Fact]
    public async Task UpdateMedia_SendsOnlyTheFieldsThatWereSet_AndNullForACleared()
    {
        ResponseMediaFile media = MediaFile();
        _handler.Respond = _ => Ok(media);

        await _client.UpdateMedia(media.ID, new() { Title = new("New"), Year = new(null) });

        Assert.Equal("""{"title":"New","year":null}""", Single().Body);
    }

    [Fact]
    public async Task DeleteMedia_DeletesTheMediaFile_On204()
    {
        _handler.Respond = _ => new(HttpStatusCode.NoContent);
        Guid media = Guid.CreateVersion7();

        await _client.DeleteMedia(media);

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal($"https://server.example/Media/{media}", request.Uri?.ToString());
    }

    [Fact]
    public async Task DeletePlaylist_DeletesThePlaylist_On204()
    {
        _handler.Respond = _ => new(HttpStatusCode.NoContent);

        await _client.DeletePlaylist(PlaylistID);

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal($"https://server.example/Playlist/{PlaylistID}", request.Uri?.ToString());
    }

    [Fact]
    public async Task AddPlaylistEntryList_PostsTheMediaFilesAndThePosition()
    {
        _handler.Respond = _ => Ok(Detail());
        Guid media = Guid.CreateVersion7();

        ResponsePlaylistDetail detail = await _client.AddPlaylistEntryList(PlaylistID, new() { MediaFileIDList = [media], Position = 2 });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"https://server.example/Playlist/{PlaylistID}/EntryList", request.Uri?.ToString());
        JsonElement body = JsonDocument.Parse(request.Body ?? "{}").RootElement;
        Assert.Equal(media, body.GetProperty("mediaFileIDList")[0].GetGuid());
        Assert.Equal(2, body.GetProperty("position").GetInt32());
        Assert.Equal(PlaylistID, detail.ID);
    }

    [Fact]
    public async Task RemovePlaylistEntryList_PostsTheEntryIDsToRemove()
    {
        _handler.Respond = _ => Ok(Detail());

        await _client.RemovePlaylistEntryList(PlaylistID, new() { EntryIDList = [EntryID] });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"https://server.example/Playlist/{PlaylistID}/EntryList/Remove", request.Uri?.ToString());
        Assert.Equal(EntryID, JsonDocument.Parse(request.Body ?? "{}").RootElement.GetProperty("entryIDList")[0].GetGuid());
    }

    [Fact]
    public async Task MovePlaylistEntry_PutsThePosition()
    {
        _handler.Respond = _ => Ok(Detail());

        await _client.MovePlaylistEntry(PlaylistID, EntryID, new() { Position = 4 });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal($"https://server.example/Playlist/{PlaylistID}/Entry/{EntryID}/Position", request.Uri?.ToString());
        Assert.Equal(4, JsonDocument.Parse(request.Body ?? "{}").RootElement.GetProperty("position").GetInt32());
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "The playlist was not found.")]
    [InlineData(HttpStatusCode.Conflict, "The playlist contains duplicate media files.")]
    public async Task AWriteThatTheServerRefuses_ThrowsApiExceptionWithItsStatusAndTitle(HttpStatusCode status, string title)
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(status, title);

        ApiException update = await Assert.ThrowsAsync<ApiException>(() => _client.UpdatePlaylist(PlaylistID, new() { AllowDuplicates = new(false) }));
        ApiException delete = await Assert.ThrowsAsync<ApiException>(() => _client.DeletePlaylist(PlaylistID));

        Assert.Equal(status, update.Status);
        Assert.Equal(title, delete.Title);
    }

    #endregion -- Writes --

    #region -- File --

    private static async Task<byte[]> ReadAll(Stream stream)
    {
        using MemoryStream copy = new();
        await stream.CopyToAsync(copy, TestContext.Current.CancellationToken);

        return copy.ToArray();
    }

    private static HttpResponseMessage File(HttpStatusCode status, FakeTrackedContent content, string? eTag = null, long? rangeFrom = null, long? total = null)
    {
        HttpResponseMessage response = new(status) { Content = content };
        if (eTag is not null) response.Headers.TryAddWithoutValidation("ETag", eTag);
        if (rangeFrom is not null) response.Content.Headers.ContentRange = new(rangeFrom.Value, (total ?? 0) - 1, total ?? 0);

        return response;
    }

    [Fact]
    public async Task GetFile_WithNoRange_GetsTheFilePath_AndReturns200WithTheBytesAndTheETag()
    {
        Guid id = Guid.CreateVersion7();
        byte[] bytes = [1, 2, 3, 4, 5];
        _handler.Respond = _ => File(HttpStatusCode.OK, new(bytes), "\"file-etag\"");

        using ApiFileResponse response = await _client.GetFile(id, null, null);

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal($"https://server.example/Media/{id}/File", request.Uri?.ToString());
        Assert.Equal("Bearer token-1", request.Authorization);
        Assert.Null(request.Range);
        Assert.Null(request.IfRange);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("\"file-etag\"", response.ETag);
        Assert.Equal(5, response.TotalBytes);
        Assert.Equal(bytes, await ReadAll(response.Content));
    }

    [Fact]
    public async Task GetTaggedFile_GetsTheDownloadPath_AndStreamsTheBody()
    {
        Guid id = Guid.CreateVersion7();
        FakeTrackedContent content = new([9, 8, 7]);
        _handler.Respond = _ => File(HttpStatusCode.OK, content);

        using ApiFileResponse response = await _client.GetTaggedFile(id);

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal($"https://server.example/Media/{id}/Download", request.Uri?.ToString());
        Assert.Equal("Bearer token-1", request.Authorization);
        Assert.Null(request.Range);
        Assert.False(content.Buffered);
        Assert.Equal([9, 8, 7], await ReadAll(response.Content));
    }

    [Fact]
    public async Task GetFile_DoesNotBufferTheBody_TheCallerReadsItFromTheStream()
    {
        FakeTrackedContent content = new([1, 2, 3]);
        _handler.Respond = _ => File(HttpStatusCode.OK, content);

        using ApiFileResponse response = await _client.GetFile(Guid.CreateVersion7(), null, null);

        Assert.False(content.Buffered);
        Assert.False(content.WasRead);
        await ReadAll(response.Content);
        Assert.True(content.WasRead);
    }

    [Fact]
    public async Task GetFile_WithARange_SendsRangeAndIfRange_AndReturns206WithTheTotalFromContentRange()
    {
        byte[] rest = [7, 8, 9];
        _handler.Respond = _ => File(HttpStatusCode.PartialContent, new(rest), "\"file-etag\"", 997, 1000);

        using ApiFileResponse response = await _client.GetFile(Guid.CreateVersion7(), 997, "\"file-etag\"");

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal("bytes=997-", request.Range);
        Assert.Equal("\"file-etag\"", request.IfRange);
        Assert.Equal(HttpStatusCode.PartialContent, response.Status);
        Assert.Equal(1000, response.TotalBytes);
        Assert.Equal(rest, await ReadAll(response.Content));
    }

    [Fact]
    public async Task GetFile_WhenTheServerAnswers200ToARangedRequest_ReturnsOKWithTheWholeFile()
    {
        byte[] whole = [1, 2, 3, 4];
        _handler.Respond = _ => File(HttpStatusCode.OK, new(whole), "\"changed\"");

        using ApiFileResponse response = await _client.GetFile(Guid.CreateVersion7(), 2, "\"old\"");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("\"changed\"", response.ETag);
        Assert.Equal(4, response.TotalBytes);
        Assert.Equal(whole, await ReadAll(response.Content));
    }

    [Fact]
    public async Task GetFile_WithARangeAndNoETag_SendsNoIfRange()
    {
        _handler.Respond = _ => File(HttpStatusCode.PartialContent, new([1]), null, 5, 6);

        using ApiFileResponse response = await _client.GetFile(Guid.CreateVersion7(), 5, null);

        Assert.Equal("bytes=5-", Single().Range);
        Assert.Null(Single().IfRange);
    }

    [Fact]
    public async Task GetFile_AnUnauthorizedResponse_RefreshesOnceAndRetriesOnce_WithTheNewToken()
    {
        _handler.Respond = request => request.Authorization == "Bearer token-2" ? File(HttpStatusCode.OK, new([1, 2])) : FakeHttpMessageHandler.Problem(HttpStatusCode.Unauthorized, "Unauthorized.");

        using ApiFileResponse response = await _client.GetFile(Guid.CreateVersion7(), null, null);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(1, _session.RefreshCount);
        Assert.Equal(["Bearer token-1", "Bearer token-2"], _handler.RequestList.Select(x => x.Authorization));
    }

    [Fact]
    public async Task GetFile_UsesTheSavedAddress_AndWithNoneSavedThrowsAndSendsNothing()
    {
        _address.Address = null;

        await Assert.ThrowsAsync<ServerAddressMissingException>(() => _client.GetFile(Guid.CreateVersion7(), null, null));

        Assert.Empty(_handler.RequestList);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "The media file was not found.")]
    [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable, "The range is outside the file.")]
    public async Task GetFile_AProblemResponse_ThrowsApiExceptionWithItsStatusAndTitle(HttpStatusCode status, string title)
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(status, title);

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _client.GetFile(Guid.CreateVersion7(), null, null));

        Assert.Equal(status, exception.Status);
        Assert.Equal(title, exception.Title);
    }

    [Fact]
    public async Task GetFile_ANetworkFailure_PropagatesAsHttpRequestException()
    {
        _handler.Failure = new HttpRequestException("The network is down.");

        await Assert.ThrowsAsync<HttpRequestException>(() => _client.GetFile(Guid.CreateVersion7(), null, null));
    }

    [Fact]
    public async Task DisposingTheFileResponse_DisposesTheStreamAndTheUnderlyingResponse()
    {
        FakeTrackedContent content = new([1, 2, 3]);
        _handler.Respond = _ => File(HttpStatusCode.OK, content);
        ApiFileResponse response = await _client.GetFile(Guid.CreateVersion7(), null, null);
        Assert.False(content.StreamDisposed);

        response.Dispose();

        Assert.True(content.StreamDisposed);
        Assert.False(response.Content.CanRead);
    }

    #endregion -- File --

    #region -- Settings and metadata lists --

    private static ResponseSetting Setting(string key, object value, SettingValueTypeEnum valueType)
    {
        return new() { Key = key, ValueType = valueType, Value = JsonSerializer.SerializeToElement(value), TimeModified = DateTime.UtcNow };
    }

    private static ResponseArtist Artist(string name = "Artist")
    {
        return new() { ID = Guid.CreateVersion7(), Name = name, MediaFileCount = 1, TimeModified = DateTime.UtcNow };
    }

    public static TheoryData<string, string> TheNewReads => new()
    {
        { "Settings", nameof(IApiClient.GetSettingList) },
        { "Artist", nameof(IApiClient.GetArtistList) },
        { "Album", nameof(IApiClient.GetAlbumList) },
        { "Genre", nameof(IApiClient.GetGenreList) }
    };

    private Task Read(string call, string? eTag)
    {
        return call switch
        {
            nameof(IApiClient.GetSettingList) => _client.GetSettingList(eTag),
            nameof(IApiClient.GetArtistList) => _client.GetArtistList(eTag),
            nameof(IApiClient.GetAlbumList) => _client.GetAlbumList(eTag),
            nameof(IApiClient.GetGenreList) => _client.GetGenreList(eTag),
            _ => throw new InvalidOperationException(call)
        };
    }

    [Theory]
    [MemberData(nameof(TheNewReads))]
    public async Task TheNewReads_SendTheBearerTokenAndTheETag_ToTheirPath(string path, string call)
    {
        _handler.Respond = _ => Ok(new List<object>());

        await Read(call, "\"abc\"");

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal($"https://server.example/{path}", request.Uri?.ToString());
        Assert.Equal("Bearer token-1", request.Authorization);
        Assert.Equal("\"abc\"", request.IfNoneMatch);
    }

    [Fact]
    public async Task GetSettingList_On200_ReturnsTheListAndTheETag_AndOn304NotModified()
    {
        _handler.Respond = _ => Ok(new List<ResponseSetting> { Setting("Volume", 40, SettingValueTypeEnum.Integer) }, "\"s1\"");

        ApiResponse<List<ResponseSetting>> response = await _client.GetSettingList(null);

        Assert.Equal("\"s1\"", response.ETag);
        ResponseSetting setting = Assert.Single(response.Value ?? []);
        Assert.Equal("Volume", setting.Key);
        Assert.Equal(40, Assert.IsType<JsonElement>(setting.Value).GetInt32());

        _handler.Respond = _ => new(HttpStatusCode.NotModified);
        ApiResponse<List<ResponseSetting>> notModified = await _client.GetSettingList("\"s1\"");
        Assert.True(notModified.IsNotModified);
        Assert.Null(notModified.Value);
    }

    [Fact]
    public async Task GetArtistListGetAlbumListAndGetGenreList_On200_ReturnTheirListAndETag_AndOn304NotModified()
    {
        _handler.Respond = _ => Ok(new List<ResponseArtist> { Artist("A") }, "\"a1\"");
        ApiResponse<List<ResponseArtist>> artist = await _client.GetArtistList(null);
        _handler.Respond = _ => Ok(new List<ResponseAlbum> { new() { ID = Guid.CreateVersion7(), Title = "B", ArtistIDList = [], MediaFileCount = 1, TimeModified = DateTime.UtcNow } }, "\"b1\"");
        ApiResponse<List<ResponseAlbum>> album = await _client.GetAlbumList(null);
        _handler.Respond = _ => Ok(new List<ResponseGenre> { new() { ID = Guid.CreateVersion7(), Name = "G", MediaFileCount = 1, TimeModified = DateTime.UtcNow } }, "\"g1\"");
        ApiResponse<List<ResponseGenre>> genre = await _client.GetGenreList(null);

        Assert.Equal(("A", "\"a1\""), (Assert.Single(artist.Value ?? []).Name, artist.ETag));
        Assert.Equal(("B", "\"b1\""), (Assert.Single(album.Value ?? []).Title, album.ETag));
        Assert.Equal(("G", "\"g1\""), (Assert.Single(genre.Value ?? []).Name, genre.ETag));

        _handler.Respond = _ => new(HttpStatusCode.NotModified);
        Assert.True((await _client.GetArtistList("\"a1\"")).IsNotModified);
        Assert.True((await _client.GetAlbumList("\"b1\"")).IsNotModified);
        Assert.True((await _client.GetGenreList("\"g1\"")).IsNotModified);
    }

    [Theory]
    [InlineData("Shuffle", "true", "{\"value\":true}")]
    [InlineData("Volume", "40", "{\"value\":40}")]
    [InlineData("Repeat", "\"Forever\"", "{\"value\":\"Forever\"}")]
    public async Task UpdateSetting_PatchesTheKeyWithTheValueAsJson_AndReturnsTheSetting(string key, string valueJson, string expectedBody)
    {
        JsonElement value = JsonDocument.Parse(valueJson).RootElement.Clone();
        _handler.Respond = _ => Ok(new { key, valueType = "Integer", value, timeModified = DateTime.UtcNow });

        ResponseSetting setting = await _client.UpdateSetting(key, value);

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal($"https://server.example/Settings/{key}", request.Uri?.ToString());
        Assert.Equal(expectedBody, request.Body);
        Assert.Equal("Bearer token-1", request.Authorization);
        Assert.Equal(key, setting.Key);
    }

    [Fact]
    public async Task MarkPlaylistPlayed_PostsToThePlayedPath_AndAccepts204()
    {
        _handler.Respond = _ => new(HttpStatusCode.NoContent);

        await _client.MarkPlaylistPlayed(PlaylistID);

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"https://server.example/Playlist/{PlaylistID}/Played", request.Uri?.ToString());
    }

    [Fact]
    public async Task UpdatePlaylist_SendsOnlyTheFieldsThatAreSet()
    {
        _handler.Respond = _ => Ok(Playlist());

        await _client.UpdatePlaylist(PlaylistID, new() { Name = new("Renamed") });
        await _client.UpdatePlaylist(PlaylistID, new() { AllowDuplicates = new(false) });
        await _client.UpdatePlaylist(PlaylistID, new() { Name = new("Mix"), AllowDuplicates = new(true) });

        Assert.Equal(["{\"name\":\"Renamed\"}", "{\"allowDuplicates\":false}", "{\"name\":\"Mix\",\"allowDuplicates\":true}"], _handler.RequestList.Select(x => x.Body));
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "The setting was not found.")]
    [InlineData(HttpStatusCode.Conflict, "Conflict.")]
    public async Task AWriteRefusedWithAProblem_ThrowsApiExceptionWithItsStatusAndTitle(HttpStatusCode status, string title)
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(status, title);

        ApiException setting = await Assert.ThrowsAsync<ApiException>(() => _client.UpdateSetting("Volume", JsonSerializer.SerializeToElement(1)));
        ApiException played = await Assert.ThrowsAsync<ApiException>(() => _client.MarkPlaylistPlayed(PlaylistID));

        Assert.Equal((status, title), (setting.Status, setting.Message));
        Assert.Equal(status, played.Status);
    }

    [Theory]
    [MemberData(nameof(TheNewReads))]
    public async Task TheNewReads_WithNoSavedAddress_ThrowServerAddressMissingException_AndSendNothing(string path, string call)
    {
        Assert.NotEmpty(path);
        _address.Address = null;

        await Assert.ThrowsAsync<ServerAddressMissingException>(() => Read(call, null));
        await Assert.ThrowsAsync<ServerAddressMissingException>(() => _client.UpdateSetting("Volume", JsonSerializer.SerializeToElement(1)));
        await Assert.ThrowsAsync<ServerAddressMissingException>(() => _client.MarkPlaylistPlayed(PlaylistID));

        Assert.Empty(_handler.RequestList);
    }

    [Fact]
    public async Task TheNewCalls_AnUnauthorizedAnswer_RefreshesOnceAndRetriesOnce_WithTheNewToken()
    {
        _handler.Respond = request => request.Authorization == "Bearer token-2" ? new(HttpStatusCode.NoContent) : FakeHttpMessageHandler.Problem(HttpStatusCode.Unauthorized, "Unauthorized.");

        await _client.MarkPlaylistPlayed(PlaylistID);

        Assert.Equal(1, _session.RefreshCount);
        Assert.Equal(2, _handler.RequestList.Count);
    }

    #endregion -- Settings and metadata lists --

    #region -- Album art --

    [Fact]
    public async Task GetAlbumArt_On200_ReturnsTheBytes_FromTheArtAddress_WithTheBearerToken()
    {
        Guid albumID = Guid.CreateVersion7();
        _handler.Respond = _ => new(HttpStatusCode.OK) { Content = new ByteArrayContent([0xFF, 0xD8, 5]) };

        byte[]? bytes = await _client.GetAlbumArt(albumID);

        Assert.Equal<byte[]?>([0xFF, 0xD8, 5], bytes);
        FakeHttpMessageHandler.RecordedRequest request = Assert.Single(_handler.RequestList);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal($"/Album/{albumID}/Art", request.Uri?.AbsolutePath);
        Assert.StartsWith("Bearer ", request.Authorization);
    }

    [Fact]
    public async Task GetAlbumArt_On404_ReturnsNull()
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(HttpStatusCode.NotFound, "Not found.");

        Assert.Null(await _client.GetAlbumArt(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task GetAlbumArt_OnAServerError_Throws()
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(HttpStatusCode.InternalServerError, "Broken.");

        await Assert.ThrowsAnyAsync<Exception>(() => _client.GetAlbumArt(Guid.CreateVersion7()));
    }

    #endregion -- Album art --

    [Fact]
    public async Task CreateArtist_PostsTheName()
    {
        Guid id = Guid.CreateVersion7();
        _handler.Respond = _ => Ok(FakeApiClient.ArtistFor(id, "Björk"));

        ResponseArtist result = await _client.CreateArtist(new() { Name = "Björk" });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://server.example/Artist", request.Uri?.ToString());
        Assert.Equal("Bearer token-1", request.Authorization);
        Assert.Equal("Björk", JsonDocument.Parse(request.Body ?? "{}").RootElement.GetProperty("name").GetString());
        Assert.Equal(id, result.ID);
    }

    [Fact]
    public async Task CreateAlbum_PostsTheTitleAndTheArtistIDList()
    {
        Guid artist = Guid.CreateVersion7();
        _handler.Respond = _ => Ok(FakeApiClient.AlbumFor(Guid.CreateVersion7(), "Homogenic", [artist]));

        await _client.CreateAlbum(new() { Title = "Homogenic", ArtistIDList = [artist] });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://server.example/Album", request.Uri?.ToString());
        JsonElement body = JsonDocument.Parse(request.Body ?? "{}").RootElement;
        Assert.Equal("Homogenic", body.GetProperty("title").GetString());
        Assert.Equal(artist, body.GetProperty("artistIDList")[0].GetGuid());
    }

    [Fact]
    public async Task UpdateAlbum_PatchesTheAlbum()
    {
        Guid id = Guid.CreateVersion7();
        _handler.Respond = _ => Ok(FakeApiClient.AlbumFor(id, "Renamed"));

        await _client.UpdateAlbum(id, new() { Title = new("Renamed") });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal($"https://server.example/Album/{id}", request.Uri?.ToString());
        Assert.Equal("""{"title":"Renamed"}""", request.Body);
    }

    [Fact]
    public async Task DeleteGenre_DeletesTheGenre_On204()
    {
        Guid id = Guid.CreateVersion7();
        _handler.Respond = _ => new(HttpStatusCode.NoContent);

        await _client.DeleteGenre(id);

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal($"https://server.example/Genre/{id}", request.Uri?.ToString());
        Assert.Equal("Bearer token-1", request.Authorization);
    }

    [Fact]
    public async Task AddArtistMediaFileList_PostsTheMediaFileIDList()
    {
        Guid id = Guid.CreateVersion7();
        Guid media = Guid.CreateVersion7();
        _handler.Respond = _ => Ok(FakeApiClient.ArtistFor(id, "Artist"));

        await _client.AddArtistMediaFileList(id, new() { MediaFileIDList = [media] });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"https://server.example/Artist/{id}/MediaFileList", request.Uri?.ToString());
        Assert.Equal(media, JsonDocument.Parse(request.Body ?? "{}").RootElement.GetProperty("mediaFileIDList")[0].GetGuid());
    }

    [Fact]
    public async Task RemoveArtistMediaFileList_PostsTheMediaFileIDList_ToTheRemovePath()
    {
        Guid id = Guid.CreateVersion7();
        Guid media = Guid.CreateVersion7();
        _handler.Respond = _ => Ok(FakeApiClient.ArtistFor(id, "Artist"));

        await _client.RemoveArtistMediaFileList(id, new() { MediaFileIDList = [media] });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"https://server.example/Artist/{id}/MediaFileList/Remove", request.Uri?.ToString());
        Assert.Equal(media, JsonDocument.Parse(request.Body ?? "{}").RootElement.GetProperty("mediaFileIDList")[0].GetGuid());
    }

    [Fact]
    public async Task AMetadataWriteThatTheServerRefuses_ThrowsApiExceptionWithItsTitle()
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(HttpStatusCode.Conflict, "An artist with that name exists.");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _client.CreateArtist(new() { Name = "Dup" }));

        Assert.Equal(HttpStatusCode.Conflict, exception.Status);
        Assert.Equal("An artist with that name exists.", exception.Title);
    }

    [Fact]
    public async Task SetAlbumArt_PutsThePictureBytes_ToTheArtPath()
    {
        Guid id = Guid.CreateVersion7();
        _handler.Respond = _ => new(HttpStatusCode.NoContent);

        await _client.SetAlbumArt(id, "abc"u8.ToArray(), "image/png");

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal($"https://server.example/Album/{id}/Art", request.Uri?.ToString());
        Assert.Equal("Bearer token-1", request.Authorization);
        Assert.Equal("abc", request.Body);
    }

    [Fact]
    public async Task SetAlbumArt_WhenTheServerRefuses_ThrowsApiExceptionWithItsTitle()
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(HttpStatusCode.BadRequest, "The picture is not an image that can be used.");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _client.SetAlbumArt(Guid.CreateVersion7(), [1], "image/png"));

        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }

    [Fact]
    public async Task MergeAlbum_PostsTheTargetID_ToTheMergePath()
    {
        Guid id = Guid.CreateVersion7();
        Guid target = Guid.CreateVersion7();
        _handler.Respond = _ => Ok(FakeApiClient.AlbumFor(target, "Target"));

        ResponseAlbum result = await _client.MergeAlbum(id, new() { TargetID = target });

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"https://server.example/Album/{id}/Merge", request.Uri?.ToString());
        Assert.Equal("Bearer token-1", request.Authorization);
        Assert.Equal(target, JsonDocument.Parse(request.Body ?? "{}").RootElement.GetProperty("targetID").GetGuid());
        Assert.Equal(target, result.ID);
    }

    [Fact]
    public async Task MergeArtist_AndMergeGenre_PostToTheirOwnMergePaths()
    {
        Guid id = Guid.CreateVersion7();
        Guid target = Guid.CreateVersion7();
        _handler.Respond = request => request.Uri?.AbsolutePath.StartsWith("/Artist") == true ? Ok(FakeApiClient.ArtistFor(target, "Target")) : Ok(FakeApiClient.GenreFor(target, "Target"));

        await _client.MergeArtist(id, new() { TargetID = target });
        await _client.MergeGenre(id, new() { TargetID = target });

        Assert.Equal([$"https://server.example/Artist/{id}/Merge", $"https://server.example/Genre/{id}/Merge"], _handler.RequestList.Select(x => x.Uri?.ToString()));
    }

    [Fact]
    public async Task AMergeThatTheServerRefuses_ThrowsApiExceptionWithItsTitle()
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(HttpStatusCode.BadRequest, "An album cannot be merged into itself.");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _client.MergeAlbum(Guid.CreateVersion7(), new() { TargetID = Guid.CreateVersion7() }));

        Assert.Equal("An album cannot be merged into itself.", exception.Title);
    }
}
