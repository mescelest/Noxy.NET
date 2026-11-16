using System.Net;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeApiClient : IApiClient
{
    // A handler gets the ETag the call was sent with and returns the scripted response, or throws to script a failure.
    public Func<string?, ApiResponse<List<ResponseMediaFile>>> LibraryHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public Func<string?, ApiResponse<List<ResponsePlaylist>>> PlaylistListHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public Func<Guid, string?, ApiResponse<ResponsePlaylistDetail>> PlaylistDetailHandler { get; set; } = (id, _) => throw new ApiException(HttpStatusCode.NotFound, "Not found.");

    public List<string?> LibraryETagList { get; } = [];

    public List<string?> PlaylistListETagList { get; } = [];

    public List<(Guid ID, string? ETag)> PlaylistDetailRequestList { get; } = [];

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
