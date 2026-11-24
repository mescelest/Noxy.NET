using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// What the playlist screens do. Every write sends the request, saves the answer into the local copy at once, and throws when the server cannot be reached or refuses (ApiException, HttpRequestException), so the page can show the reason; nothing changes locally then.
public interface IPlaylistService
{
    Task<ResponsePlaylist> Create(string name, bool allowDuplicates = true);

    // Syncs the playlist's detail. It downloads nothing: that is "Download all", or playing. Offline it uses the stored detail. A playlist the server no longer has is removed locally.
    Task Open(Guid id);

    // Syncs the detail again.
    Task Refresh(Guid id);

    Task DownloadAll(Guid id);

    Task Play(Guid id, Guid? startEntryID = null);

    Task Rename(Guid id, string name);

    Task SetAllowDuplicates(Guid id, bool allow);

    // The name and the duplicates rule in one update; the server treats a field that has not changed as no change. The hue is sent only when given: a value chooses it, a null clears it to the default, and leaving it out keeps it.
    Task Edit(Guid id, string name, bool allowDuplicates, Optional<int?> artHue = default);

    Task RemoveEntryList(Guid id, List<Guid> entryIDList);

    Task MoveEntry(Guid id, Guid entryID, int newPosition);

    // Moves the playlist to the position in the user's own order and saves the answer, the whole list in its new order.
    Task Move(Guid id, int newPosition);

    Task Delete(Guid id);
}
