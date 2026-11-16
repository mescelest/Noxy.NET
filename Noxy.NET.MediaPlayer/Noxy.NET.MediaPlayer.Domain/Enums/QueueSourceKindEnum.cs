using System.Text.Json.Serialization;
using Noxy.NET.MediaPlayer.Domain.Converters;

namespace Noxy.NET.MediaPlayer.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumNameConverter<QueueSourceKindEnum>))]
public enum QueueSourceKindEnum
{
    Library,
    Playlist
}
