namespace Noxy.NET.Abstractions;

public class UniversalValue(object? value = null)
{
    public virtual object? Value { get; set; } = value;

    public sbyte? SByteValue
    {
        get => Value as sbyte?;
        set => Value = value;
    }

    public byte? ByteValue
    {
        get => Value as byte?;
        set => Value = value;
    }

    public bool? BoolValue
    {
        get => Value as bool?;
        set => Value = value;
    }

    public short? ShortValue
    {
        get => Value as short?;
        set => Value = value;
    }

    public int? IntegerValue
    {
        get => Value as int?;
        set => Value = value;
    }

    public long? LongValue
    {
        get => Value as long?;
        set => Value = value;
    }

    public ushort? UShortValue
    {
        get => Value as ushort?;
        set => Value = value;
    }

    public int? UIntegerValue
    {
        get => Value as int?;
        set => Value = value;
    }

    public ulong? ULongValue
    {
        get => Value as ulong?;
        set => Value = value;
    }

    public float? FloatValue
    {
        get => Value as float?;
        set => Value = value;
    }

    public decimal? DecimalValue
    {
        get => Value as decimal?;
        set => Value = value;
    }

    public double? DoubleValue
    {
        get => Value as double?;
        set => Value = value;
    }

    public Guid? GuidValue
    {
        get => Value as Guid?;
        set => Value = value;
    }

    public DateTime? DateTimeValue
    {
        get => Value as DateTime?;
        set => Value = value;
    }

    public DateTimeOffset? DateTimeOffsetValue
    {
        get => Value as DateTimeOffset?;
        set => Value = value;
    }

    public string? StringValue
    {
        get => Value?.ToString();
        set => Value = value;
    }
}
