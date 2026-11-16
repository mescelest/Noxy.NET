using System.Text.Json.Serialization;
using Noxy.NET.MediaPlayer.Domain.Converters;

namespace Noxy.NET.MediaPlayer.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumNameConverter<PlayerCommandEnum>))]
public enum PlayerCommandEnum
{
    Play,
    Pause,
    Seek,
    Next,
    Previous,
    JumpTo,
    SetVolume,
    SetShuffle,
    SetRepeat,
    PlayLibrary,
    PlayPlaylist
}
