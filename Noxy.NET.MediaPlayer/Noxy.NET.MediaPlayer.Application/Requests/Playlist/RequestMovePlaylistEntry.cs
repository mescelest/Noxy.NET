using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Requests.Playlist;

public class RequestMovePlaylistEntry
{
    [Range(0, int.MaxValue)]
    public int Position { get; set; }

    public CommandPlaylistEntryMove ToCommand(Guid userID, Guid id, Guid entryID)
    {
        return new(userID, id, entryID, Position);
    }
}
