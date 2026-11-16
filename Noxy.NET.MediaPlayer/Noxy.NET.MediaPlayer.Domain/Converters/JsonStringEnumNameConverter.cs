using System.Text.Json.Serialization;

namespace Noxy.NET.MediaPlayer.Domain.Converters;

public class JsonStringEnumNameConverter<T>() : JsonStringEnumConverter<T>(null, false) where T : struct, Enum;
