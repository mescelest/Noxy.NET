using System.Text.Json.Serialization;
using Noxy.NET.MediaPlayer.Domain.Converters;

namespace Noxy.NET.MediaPlayer.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumNameConverter<CommandResultEnum>))]
public enum CommandResultEnum
{
    Sent,
    NoActiveDevice,
    Invalid
}
