using System.Text.Json.Serialization;
using Noxy.NET.MediaPlayer.Domain.Converters;

namespace Noxy.NET.MediaPlayer.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumNameConverter<QueueEditEnum>))]
public enum QueueEditEnum
{
    PlayNext,
    AddToQueue,
    Remove,
    Move,
    ReplaceSource,
    RegenerateSource,
    InsertSourceItem,
    RemoveSourceItem,
    Advance,
    Previous
}
