using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IPlaylistService
{
    Task<PlaylistLibrary> GetPlaylistLibrary(Guid userID, string? ifNoneMatch, CancellationToken cancellationToken = default);

    Task<PlaylistView> GetPlaylist(Guid userID, Guid id, string? ifNoneMatch, CancellationToken cancellationToken = default);

    Task<ResponsePlaylist> Create(Guid userID, RequestCreatePlaylist request, CancellationToken cancellationToken = default);

    Task<ResponsePlaylist> Update(Guid userID, Guid id, RequestUpdatePlaylist request, CancellationToken cancellationToken = default);

    Task Delete(Guid userID, Guid id, CancellationToken cancellationToken = default);

    Task<ResponsePlaylistDetail> AddEntryList(Guid userID, Guid id, RequestAddPlaylistEntry request, CancellationToken cancellationToken = default);

    Task<ResponsePlaylistDetail> RemoveEntryList(Guid userID, Guid id, RequestRemovePlaylistEntry request, CancellationToken cancellationToken = default);

    Task<ResponsePlaylistDetail> MoveEntry(Guid userID, Guid id, Guid entryID, RequestMovePlaylistEntry request, CancellationToken cancellationToken = default);
}
