using System.Text.Json.Serialization;
using Noxy.NET.MediaPlayer.Application.Attributes;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Declarations;

namespace Noxy.NET.MediaPlayer.Application.Requests.Playlist;

public class RequestUpdatePlaylist
{
    [OptionalNotBlank]
    [OptionalMaxLength(255)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string?> Name { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<bool> AllowDuplicates { get; set; }

    [OptionalRange(PlaylistArtHue.Min, PlaylistArtHue.Max)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<int?> ArtHue { get; set; }

    public CommandPlaylistUpdate ToCommand(Guid userID, Guid id)
    {
        return new(userID, id, Name, AllowDuplicates, ArtHue);
    }
}
