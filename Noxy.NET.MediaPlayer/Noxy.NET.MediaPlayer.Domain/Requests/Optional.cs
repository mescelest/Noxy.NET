using System.Text.Json.Serialization;
using Noxy.NET.MediaPlayer.Domain.Interfaces;

namespace Noxy.NET.MediaPlayer.Domain.Requests;

[JsonConverter(typeof(OptionalJsonConverterFactory))]
public readonly struct Optional<T>(T? value) : IOptional
{
    // The default value means the field was left out of the request.

    public bool HasValue { get; } = true;
    public T? Value { get; } = value;

    object? IOptional.UntypedValue => Value;
}
