using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// What the playlist screens do. Every write sends the request, saves the answer into the local copy at once, and throws when the server cannot be reached or refuses (ApiException, HttpRequestException), so the page can show the reason; nothing changes locally then.
public interface IPlaylistService
{
    Task<ResponsePlaylist> Create(string name);

    // Syncs the playlist's detail, then starts the downloads of its media files that are not on the device. This is the only thing that downloads for a playlist. Offline it uses the stored detail. A playlist the server no longer has is removed locally.
    Task Open(Guid id);

    // Syncs the detail again.
    Task Refresh(Guid id);

    Task DownloadAll(Guid id);

    Task Play(Guid id, Guid? startEntryID = null);

    Task Rename(Guid id, string name);

    Task SetAllowDuplicates(Guid id, bool allow);

    // Writes the playlist's own mode; null means follow the global setting.
    Task SetShuffle(Guid id, ShuffleModeEnum? mode);

    Task SetRepeat(Guid id, RepeatModeEnum? mode);

    Task RemoveEntryList(Guid id, List<Guid> entryIDList);

    Task MoveEntry(Guid id, Guid entryID, int newPosition);

    Task Delete(Guid id);
}
