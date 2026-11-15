using Noxy.NET.MediaPlayer.Application.Attributes;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Models;

namespace Noxy.NET.MediaPlayer.Application.Requests.Playlist;

public class RequestUpdatePlaylist
{
    [OptionalNotBlank]
    [OptionalMaxLength(255)]
    public Optional<string?> Name { get; set; }

    public Optional<bool> AllowDuplicates { get; set; }

    public CommandPlaylistUpdate ToCommand(Guid userID, Guid id)
    {
        return new(userID, id, Name, AllowDuplicates);
    }
}
