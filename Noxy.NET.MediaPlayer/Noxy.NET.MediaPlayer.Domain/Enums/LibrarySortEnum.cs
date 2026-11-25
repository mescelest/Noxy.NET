using System.Text.Json.Serialization;
using Noxy.NET.MediaPlayer.Domain.Converters;

namespace Noxy.NET.MediaPlayer.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumNameConverter<LibrarySortEnum>))]
public enum LibrarySortEnum
{
    Title,
    Artist,
    Album,
    Year,
    TimeCreated,
    Genre
}
