using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Requests.Playlist;

public class RequestRemovePlaylistEntry
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public List<Guid> EntryIDList { get; set; } = [];

    public CommandPlaylistEntryListRemove ToCommand(Guid userID, Guid id)
    {
        return new(userID, id, EntryIDList);
    }
}
