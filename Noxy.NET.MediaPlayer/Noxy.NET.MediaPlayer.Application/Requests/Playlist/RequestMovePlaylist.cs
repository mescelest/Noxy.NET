using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Requests.Playlist;

public class RequestMovePlaylist
{
    [Range(0, int.MaxValue)]
    public int Position { get; set; }

    public CommandPlaylistMove ToCommand(Guid userID, Guid id)
    {
        return new(userID, id, Position);
    }
}
