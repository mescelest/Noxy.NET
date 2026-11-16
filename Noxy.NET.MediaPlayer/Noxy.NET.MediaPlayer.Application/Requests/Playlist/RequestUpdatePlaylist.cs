using System.Text.Json.Serialization;
using Noxy.NET.MediaPlayer.Application.Attributes;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Application.Requests.Playlist;

public class RequestUpdatePlaylist
{
    [OptionalNotBlank]
    [OptionalMaxLength(255)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string?> Name { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<bool> AllowDuplicates { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<ShuffleModeEnum?> Shuffle { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<RepeatModeEnum?> Repeat { get; set; }

    public CommandPlaylistUpdate ToCommand(Guid userID, Guid id)
    {
        return new(userID, id, Name, AllowDuplicates, Shuffle, Repeat);
    }
}
