using System.Net;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeApiClient : IApiClient
{
    // A handler gets the ETag the call was sent with and returns the scripted response, or throws to script a failure.
    public Func<string?, ApiResponse<List<ResponseMediaFile>>> LibraryHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public Func<string?, ApiResponse<List<ResponsePlaylist>>> PlaylistListHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public Func<Guid, string?, ApiResponse<ResponsePlaylistDetail>> PlaylistDetailHandler { get; set; } = (_, _) => throw new ApiException(HttpStatusCode.NotFound, "Not found.");

    // Answers each GetFile call, and may wait on a gate or throw to script a failure. The default is a 404.
    public Func<Guid, long?, string?, Task<ApiFileResponse>> FileHandler { get; set; } = (_, _, _) => throw new ApiException(HttpStatusCode.NotFound, "Not found.");

    public List<(Guid ID, long? RangeStart, string? IfRange)> FileRequestList { get; } = [];

    private readonly Lock _fileRequestLock = new();

    public List<string?> LibraryETagList { get; } = [];

    public List<string?> PlaylistListETagList { get; } = [];

    public List<(Guid ID, string? ETag)> PlaylistDetailRequestList { get; } = [];

    public Task<ApiFileResponse> GetFile(Guid id, long? rangeStart, string? ifRange)
    {
        lock (_fileRequestLock) FileRequestList.Add((id, rangeStart, ifRange));
        return FileHandler(id, rangeStart, ifRange);
    }

    public Task<ApiResponse<List<ResponseMediaFile>>> GetLibrary(string? eTag)
    {
        LibraryETagList.Add(eTag);
        return Task.FromResult(LibraryHandler(eTag));
    }

    public Task<ApiResponse<List<ResponsePlaylist>>> GetPlaylistList(string? eTag)
    {
        PlaylistListETagList.Add(eTag);
        return Task.FromResult(PlaylistListHandler(eTag));
    }

    public Task<ApiResponse<ResponsePlaylistDetail>> GetPlaylistDetail(Guid id, string? eTag)
    {
        PlaylistDetailRequestList.Add((id, eTag));
        return Task.FromResult(PlaylistDetailHandler(id, eTag));
    }

    public Func<string?, ApiResponse<List<ResponseSetting>>> SettingListHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public Func<string, JsonElement, ResponseSetting> UpdateSettingHandler { get; set; } = (key, _) => throw new ApiException(HttpStatusCode.NotFound, $"The setting {key} was not found.");

    public Func<string?, ApiResponse<List<ResponseArtist>>> ArtistListHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public Func<string?, ApiResponse<List<ResponseAlbum>>> AlbumListHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public Func<string?, ApiResponse<List<ResponseGenre>>> GenreListHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public List<string?> SettingListETagList { get; } = [];

    public List<(string Key, JsonElement Value)> UpdateSettingRequestList { get; } = [];

    public List<string?> ArtistListETagList { get; } = [];

    public List<string?> AlbumListETagList { get; } = [];

    public List<string?> GenreListETagList { get; } = [];

    public List<Guid> PlayedRequestList { get; } = [];

    public Task<ApiResponse<List<ResponseSetting>>> GetSettingList(string? eTag)
    {
        SettingListETagList.Add(eTag);
        return Task.FromResult(SettingListHandler(eTag));
    }

    public Task<ResponseSetting> UpdateSetting(string key, JsonElement value)
    {
        UpdateSettingRequestList.Add((key, value));
        return Task.FromResult(UpdateSettingHandler(key, value));
    }

    public Task<ApiResponse<List<ResponseArtist>>> GetArtistList(string? eTag)
    {
        ArtistListETagList.Add(eTag);
        return Task.FromResult(ArtistListHandler(eTag));
    }

    public Task<ApiResponse<List<ResponseAlbum>>> GetAlbumList(string? eTag)
    {
        AlbumListETagList.Add(eTag);
        return Task.FromResult(AlbumListHandler(eTag));
    }

    public Task<ApiResponse<List<ResponseGenre>>> GetGenreList(string? eTag)
    {
        GenreListETagList.Add(eTag);
        return Task.FromResult(GenreListHandler(eTag));
    }

    public Task MarkPlaylistPlayed(Guid id)
    {
        PlayedRequestList.Add(id);
        return Task.CompletedTask;
    }

    public Task<ResponsePlaylist> CreatePlaylist(RequestCreatePlaylist request)
    {
        throw new NotSupportedException();
    }

    public Task<ResponsePlaylist> UpdatePlaylist(Guid id, RequestUpdatePlaylist request)
    {
        throw new NotSupportedException();
    }

    public Task DeletePlaylist(Guid id)
    {
        throw new NotSupportedException();
    }

    public Task<ResponsePlaylistDetail> AddPlaylistEntryList(Guid id, RequestAddPlaylistEntry request)
    {
        throw new NotSupportedException();
    }

    public Task<ResponsePlaylistDetail> RemovePlaylistEntryList(Guid id, RequestRemovePlaylistEntry request)
    {
        throw new NotSupportedException();
    }

    public Task<ResponsePlaylistDetail> MovePlaylistEntry(Guid id, Guid entryID, RequestMovePlaylistEntry request)
    {
        throw new NotSupportedException();
    }
}
