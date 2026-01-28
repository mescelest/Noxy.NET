using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using Noxy.NET.Enums;

namespace Noxy.NET.Models;

public class JsonProperty
{
    [JsonConstructor]
    public JsonProperty()
    {
    }

    public JsonProperty(object? value)
    {
        Value = ConvertToJsonElement(value);
        Type = value switch
        {
            null => JsonPropertyTypeEnum.Null,
            sbyte => JsonPropertyTypeEnum.SByte,
            byte => JsonPropertyTypeEnum.Byte,
            bool => JsonPropertyTypeEnum.Boolean,
            short => JsonPropertyTypeEnum.Short,
            int => JsonPropertyTypeEnum.Integer,
            long => JsonPropertyTypeEnum.Long,
            ushort => JsonPropertyTypeEnum.UShort,
            uint => JsonPropertyTypeEnum.UInteger,
            ulong => JsonPropertyTypeEnum.ULong,
            float => JsonPropertyTypeEnum.Single,
            decimal => JsonPropertyTypeEnum.Decimal,
            double => JsonPropertyTypeEnum.Double,
            string => JsonPropertyTypeEnum.String,
            Guid => JsonPropertyTypeEnum.Guid,
            DateTime => JsonPropertyTypeEnum.DateTime,
            DateTimeOffset => JsonPropertyTypeEnum.DateTimeOffset,
            IList => JsonPropertyTypeEnum.Array,
            IDictionary => JsonPropertyTypeEnum.Object,
            _ => JsonPropertyTypeEnum.Unknown
        };
    }

    public JsonElement Value { get; set; }
    public JsonPropertyTypeEnum Type { get; set; }

    public object? GetValue()
    {
        return Type switch
        {
            JsonPropertyTypeEnum.Unknown => Value.GetRawText(),
            JsonPropertyTypeEnum.Null => null,
            JsonPropertyTypeEnum.SByte => AsSByte(),
            JsonPropertyTypeEnum.Byte => AsByte(),
            JsonPropertyTypeEnum.Boolean => AsBoolean(),
            JsonPropertyTypeEnum.Short => AsShort(),
            JsonPropertyTypeEnum.Integer => AsInteger(),
            JsonPropertyTypeEnum.Long => AsLong(),
            JsonPropertyTypeEnum.UShort => AsUShort(),
            JsonPropertyTypeEnum.UInteger => AsUInteger(),
            JsonPropertyTypeEnum.ULong => AsULong(),
            JsonPropertyTypeEnum.Single => AsSingle(),
            JsonPropertyTypeEnum.Decimal => AsDecimal(),
            JsonPropertyTypeEnum.Double => AsDouble(),
            JsonPropertyTypeEnum.String => AsString(),
            JsonPropertyTypeEnum.Guid => AsGUID(),
            JsonPropertyTypeEnum.DateTime => AsDateTime(),
            JsonPropertyTypeEnum.DateTimeOffset => AsDateTimeOffset(),
            JsonPropertyTypeEnum.Array => AsArray(),
            JsonPropertyTypeEnum.Object => AsDictionary(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public sbyte? AsSByte()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            JsonValueKind.Number => Value.TryGetInt32(out int value) && value is >= sbyte.MinValue and <= sbyte.MaxValue ? (sbyte)value : null,
            JsonValueKind.String => sbyte.TryParse(Value.GetString(), out sbyte value) ? value : null,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public byte? AsByte()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            JsonValueKind.Number => Value.TryGetInt32(out int value) && value is >= byte.MinValue and <= byte.MaxValue ? (byte)value : null,
            JsonValueKind.String => byte.TryParse(Value.GetString(), out byte value) ? value : null,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public bool? AsBoolean()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => GetNumberAsBoolean(),
            JsonValueKind.String => bool.TryParse(Value.GetString(), out bool value) ? value : null,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public short? AsShort()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            JsonValueKind.Number => Value.TryGetInt16(out short value) ? value : null,
            JsonValueKind.String => short.TryParse(Value.GetString(), out short value) ? value : null,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public int? AsInteger()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            JsonValueKind.Number => Value.TryGetInt32(out int value) ? value : null,
            JsonValueKind.String => int.TryParse(Value.GetString(), out int value) ? value : null,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public long? AsLong()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            JsonValueKind.Number => Value.TryGetInt64(out long value) ? value : null,
            JsonValueKind.String => long.TryParse(Value.GetString(), out long value) ? value : null,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public ushort? AsUShort()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            JsonValueKind.Number => Value.TryGetUInt16(out ushort value) ? value : null,
            JsonValueKind.String => ushort.TryParse(Value.GetString(), out ushort value) ? value : null,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public uint? AsUInteger()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            JsonValueKind.Number => Value.TryGetUInt32(out uint value) ? value : null,
            JsonValueKind.String => uint.TryParse(Value.GetString(), out uint value) ? value : null,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public ulong? AsULong()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            JsonValueKind.Number => Value.TryGetUInt64(out ulong value) ? value : null,
            JsonValueKind.String => ulong.TryParse(Value.GetString(), out ulong value) ? value : null,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public float? AsSingle()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            JsonValueKind.Number => Value.TryGetSingle(out float value) ? value : null,
            JsonValueKind.String => float.TryParse(Value.GetString(), out float value) ? value : null,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public decimal? AsDecimal()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            JsonValueKind.Number => Value.TryGetDecimal(out decimal value) ? value : null,
            JsonValueKind.String => decimal.TryParse(Value.GetString(), out decimal value) ? value : null,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object? AsDouble()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            JsonValueKind.Number => Value.TryGetDouble(out double value) ? value : null,
            JsonValueKind.String => double.TryParse(Value.GetString(), out double value) ? value : null,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public string? AsString()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => Value.GetRawText(),
            JsonValueKind.String => Value.GetString(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public Guid? AsGUID()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => throw new InvalidOperationException(),
            JsonValueKind.False => throw new InvalidOperationException(),
            JsonValueKind.Number => throw new InvalidOperationException(),
            JsonValueKind.String => Value.GetGuid(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public DateTime? AsDateTime()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => throw new InvalidOperationException(),
            JsonValueKind.False => throw new InvalidOperationException(),
            JsonValueKind.Number => throw new InvalidOperationException(),
            JsonValueKind.String => Value.GetDateTime(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public DateTimeOffset? AsDateTimeOffset()
    {
        return Value.ValueKind switch
        {
            JsonValueKind.True => throw new InvalidOperationException(),
            JsonValueKind.False => throw new InvalidOperationException(),
            JsonValueKind.Number => Value.TryGetInt64(out long value) ? DateTimeOffset.FromUnixTimeSeconds(value) : null,
            JsonValueKind.String => Value.GetDateTimeOffset(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object?[] AsArray()
    {
        return Value.EnumerateArray().Select(x => x.Deserialize<JsonProperty>()?.GetValue()).ToArray();
    }

    public Dictionary<string, object?> AsDictionary()
    {
        return Value.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.Deserialize<JsonProperty>()?.GetValue());
    }

    private bool? GetNumberAsBoolean()
    {
        if (Value.TryGetInt32(out int valueInt))
        {
            return valueInt switch
            {
                0 => false,
                1 => true,
                _ => null
            };
        }

        if (Value.TryGetDecimal(out decimal valueDecimal))
        {
            return valueDecimal switch
            {
                0m => false,
                1m => true,
                _ => null
            };
        }

        return null;
    }

    private static JsonElement ConvertToJsonElement(object? value)
    {
        if (value is IList list)
        {
            List<JsonProperty> result = [];
            foreach (object? item in list)
            {
                result.Add(new(item));
            }

            return JsonDocument.Parse(JsonSerializer.Serialize(result)).RootElement;
        }

        if (value is IDictionary dict)
        {
            Dictionary<string, JsonProperty> result = [];
            foreach (DictionaryEntry item in dict)
            {
                result.Add(item.Key.ToString() ?? string.Empty, new(item));
            }

            return JsonDocument.Parse(JsonSerializer.Serialize(result)).RootElement;
        }

        return JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement;
    }
}
