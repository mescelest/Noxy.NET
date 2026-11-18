using System.Text.Json;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Every call goes to the saved server address with the signed-in user's access token, and throws ServerAddressMissingException when no address is saved. A response that is not a success (other than 304 on the reads) throws ApiException with the server's status and title; a network failure propagates as HttpRequestException.
public interface IApiClient
{
    // With an ETag the server answers 304 when nothing changed: the response then has no value.
    Task<ApiResponse<List<ResponseMediaFile>>> GetLibrary(string? eTag);

    Task<ApiResponse<List<ResponsePlaylist>>> GetPlaylistList(string? eTag);

    Task<ApiResponse<ResponsePlaylistDetail>> GetPlaylistDetail(Guid id, string? eTag);

    // The media file's bytes, streamed: the body is not read until the caller reads Content. With rangeStart the server is asked for the bytes from there on, and with ifRange (the ETag of the bytes already held) it answers 206 only when the file is still that one, else 200 with the whole file. The caller disposes the response.
    Task<ApiFileResponse> GetFile(Guid id, long? rangeStart, string? ifRange);

    // The tagged copy the server builds (GET /Media/{id}/Download), streamed like GetFile and without a range. The caller disposes the response.
    Task<ApiFileResponse> GetTaggedFile(Guid id);

    // The user's settings, each with its value as the JSON the server sent. Same ETag rules as the library.
    Task<ApiResponse<List<ResponseSetting>>> GetSettingList(string? eTag);

    // Writes one setting and returns it as the server stored it. The value is the JSON of a boolean, an integer or an enum member name.
    Task<ResponseSetting> UpdateSetting(string key, JsonElement value);

    Task<ApiResponse<List<ResponseArtist>>> GetArtistList(string? eTag);

    Task<ApiResponse<List<ResponseAlbum>>> GetAlbumList(string? eTag);

    Task<ApiResponse<List<ResponseGenre>>> GetGenreList(string? eTag);

    // Tells the server a playlist started playing. The server answers 204.
    Task MarkPlaylistPlayed(Guid id);

    Task<ResponsePlaylist> CreatePlaylist(RequestCreatePlaylist request);

    Task<ResponsePlaylist> UpdatePlaylist(Guid id, RequestUpdatePlaylist request);

    Task DeletePlaylist(Guid id);

    // Deletes the media file from the library (DELETE /Media/{id}, 204). The server pushes the change to every device.
    Task DeleteMedia(Guid id);

    Task<ResponsePlaylistDetail> AddPlaylistEntryList(Guid id, RequestAddPlaylistEntry request);

    Task<ResponsePlaylistDetail> RemovePlaylistEntryList(Guid id, RequestRemovePlaylistEntry request);

    Task<ResponsePlaylistDetail> MovePlaylistEntry(Guid id, Guid entryID, RequestMovePlaylistEntry request);
}
