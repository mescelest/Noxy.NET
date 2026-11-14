using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noxy.NET.MediaPlayer.Domain.Requests;

public class OptionalJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type valueType = typeToConvert.GetGenericArguments()[0];

        return (JsonConverter?)Activator.CreateInstance(typeof(OptionalJsonConverter<>).MakeGenericType(valueType));
    }
}
