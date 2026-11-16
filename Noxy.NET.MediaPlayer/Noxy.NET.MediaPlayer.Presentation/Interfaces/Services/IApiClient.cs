using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Every call goes to the saved server address with the signed-in user's access token, and throws ServerAddressMissingException when no address is saved. A response that is not a success (other than 304 on the reads) throws ApiException with the server's status and title; a network failure propagates as HttpRequestException.
public interface IApiClient
{
    // With an ETag the server answers 304 when nothing changed: the response then has no value.
    Task<ApiResponse<List<ResponseMediaFile>>> GetLibrary(string? eTag);

    Task<ApiResponse<List<ResponsePlaylist>>> GetPlaylistList(string? eTag);

    Task<ApiResponse<ResponsePlaylistDetail>> GetPlaylistDetail(Guid id, string? eTag);

    Task<ResponsePlaylist> CreatePlaylist(RequestCreatePlaylist request);

    Task<ResponsePlaylist> UpdatePlaylist(Guid id, RequestUpdatePlaylist request);

    Task DeletePlaylist(Guid id);

    Task<ResponsePlaylistDetail> AddPlaylistEntryList(Guid id, RequestAddPlaylistEntry request);

    Task<ResponsePlaylistDetail> RemovePlaylistEntryList(Guid id, RequestRemovePlaylistEntry request);

    Task<ResponsePlaylistDetail> MovePlaylistEntry(Guid id, Guid entryID, RequestMovePlaylistEntry request);
}
