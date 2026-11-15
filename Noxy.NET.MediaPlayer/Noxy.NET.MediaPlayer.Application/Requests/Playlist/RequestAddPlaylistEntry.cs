using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Requests.Playlist;

public class RequestAddPlaylistEntry
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public List<Guid> MediaFileIDList { get; set; } = [];

    // Where the first media file goes. Left out, the media files are added at the end.
    [Range(0, int.MaxValue)]
    public int? Position { get; set; }

    public CommandPlaylistEntryListAdd ToCommand(Guid userID, Guid id)
    {
        return new(userID, id, MediaFileIDList, Position);
    }
}
