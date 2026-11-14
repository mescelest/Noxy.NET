using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noxy.NET.MediaPlayer.Domain.Requests;

public class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    // A JSON null is a value too ("clear this field"), so it must reach Read.
    public override bool HandleNull => true;

    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new(JsonSerializer.Deserialize<T>(ref reader, options));
    }

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
