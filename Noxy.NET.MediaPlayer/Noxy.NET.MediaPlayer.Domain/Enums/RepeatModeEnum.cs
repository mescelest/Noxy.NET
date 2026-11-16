using System.Text.Json.Serialization;
using Noxy.NET.MediaPlayer.Domain.Converters;

namespace Noxy.NET.MediaPlayer.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumNameConverter<RepeatModeEnum>))]
public enum RepeatModeEnum
{
    No,
    Once,
    Forever
}
