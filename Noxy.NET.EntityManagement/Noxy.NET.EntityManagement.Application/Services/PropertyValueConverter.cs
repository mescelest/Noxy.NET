using System.Diagnostics.CodeAnalysis;
using Noxy.NET.EntityManagement.Application.Enums;
using Noxy.NET.EntityManagement.Application.Models;
using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Definitions.Models;
using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities;

namespace Noxy.NET.EntityManagement.Application.Services;

// Turns a value that was given into the row that holds it, in the type of the property, and back. Values from outside (JSON, a form) come in all sorts of numeric types, so the whole numbers are accepted for a whole number and for a decimal; a float or a double is not, because it would not keep its digits.
internal static class PropertyValueConverter
{
    // No value: nothing, or a string with nothing in it (what an empty input of a form gives).
    public static bool IsEmpty(object? value) => value is null || value is string text && string.IsNullOrWhiteSpace(text);

    public static bool TryCreate(DefinitionProperty property, object value, Guid ownerID, Guid userID, DateTime time, [NotNullWhen(true)] out DataProperty? row, [NotNullWhen(false)] out DataError? error)
    {
        row = null;
        error = null;

        switch (property.Kind)
        {
            case PropertyKindEnum.Boolean when value is bool boolean:
                row = new DataPropertyBoolean { OwnerID = ownerID, PropertyIdentifier = property.Identifier, Value = boolean, CreatedByUserID = userID, TimeCreated = time };
                return true;

            case PropertyKindEnum.DateTime when TryGetDateTime(value, out DateTime dateTime):
                row = new DataPropertyDateTime { OwnerID = ownerID, PropertyIdentifier = property.Identifier, Value = dateTime, CreatedByUserID = userID, TimeCreated = time };
                return true;

            case PropertyKindEnum.Decimal when TryGetDecimal(value, out decimal number):
                row = new DataPropertyDecimal { OwnerID = ownerID, PropertyIdentifier = property.Identifier, Value = number, CreatedByUserID = userID, TimeCreated = time };
                return true;

            case PropertyKindEnum.Integer when TryGetLong(value, out long whole):
                row = new DataPropertyInteger { OwnerID = ownerID, PropertyIdentifier = property.Identifier, Value = whole, CreatedByUserID = userID, TimeCreated = time };
                return true;

            case PropertyKindEnum.String when value is string text:
                if (text.Length > (property.MaxLength ?? BaseData.TextMaxLength))
                {
                    error = new(property.Identifier, DataErrorEnum.TooLong, $"The property '{property.Identifier}' can be at most {property.MaxLength ?? BaseData.TextMaxLength} characters.");
                    return false;
                }

                row = new DataPropertyString { OwnerID = ownerID, PropertyIdentifier = property.Identifier, Value = text, CreatedByUserID = userID, TimeCreated = time };
                return true;

            case PropertyKindEnum.Image when value is string reference:
                if (reference.Length > BaseData.TextMaxLength)
                {
                    error = new(property.Identifier, DataErrorEnum.TooLong, $"The property '{property.Identifier}' can be at most {BaseData.TextMaxLength} characters.");
                    return false;
                }

                row = new DataPropertyImage { OwnerID = ownerID, PropertyIdentifier = property.Identifier, Value = reference, CreatedByUserID = userID, TimeCreated = time };
                return true;

            default:
                error = new(property.Identifier, DataErrorEnum.WrongType, $"The property '{property.Identifier}' is a {property.Kind}, and a {value.GetType().Name} is not one.");
                return false;
        }
    }

    // Whether the value is the same as the one already stored. Decimals are compared by their number (1.0 is 1.00), strings exactly.
    public static bool AreEqual(DataProperty current, DataProperty candidate)
    {
        return current.GetType() == candidate.GetType() && Equals(GetValue(current), GetValue(candidate));
    }

    public static object GetValue(DataProperty row) => row switch
    {
        DataPropertyBoolean x => x.Value,
        DataPropertyDateTime x => x.Value,
        DataPropertyDecimal x => x.Value,
        DataPropertyImage x => x.Value,
        DataPropertyInteger x => x.Value,
        DataPropertyString x => x.Value,
        _ => throw new NotSupportedException($"{row.GetType().Name} is not a type of property.")
    };

    public static PropertyKindEnum GetKind(DataProperty row) => row switch
    {
        DataPropertyBoolean => PropertyKindEnum.Boolean,
        DataPropertyDateTime => PropertyKindEnum.DateTime,
        DataPropertyDecimal => PropertyKindEnum.Decimal,
        DataPropertyImage => PropertyKindEnum.Image,
        DataPropertyInteger => PropertyKindEnum.Integer,
        DataPropertyString => PropertyKindEnum.String,
        _ => throw new NotSupportedException($"{row.GetType().Name} is not a type of property.")
    };

    public static object GetValue(HistoryProperty row) => row switch
    {
        HistoryPropertyBoolean x => x.Value,
        HistoryPropertyDateTime x => x.Value,
        HistoryPropertyDecimal x => x.Value,
        HistoryPropertyImage x => x.Value,
        HistoryPropertyInteger x => x.Value,
        HistoryPropertyString x => x.Value,
        _ => throw new NotSupportedException($"{row.GetType().Name} is not a type of property.")
    };

    public static PropertyKindEnum GetKind(HistoryProperty row) => row switch
    {
        HistoryPropertyBoolean => PropertyKindEnum.Boolean,
        HistoryPropertyDateTime => PropertyKindEnum.DateTime,
        HistoryPropertyDecimal => PropertyKindEnum.Decimal,
        HistoryPropertyImage => PropertyKindEnum.Image,
        HistoryPropertyInteger => PropertyKindEnum.Integer,
        HistoryPropertyString => PropertyKindEnum.String,
        _ => throw new NotSupportedException($"{row.GetType().Name} is not a type of property.")
    };

    // A time is kept in UTC: local time is converted, and a time of no kind (what JSON without a "Z" gives) is taken to be UTC already.
    private static bool TryGetDateTime(object value, out DateTime result)
    {
        switch (value)
        {
            case DateTime dateTime:
                result = dateTime.Kind == DateTimeKind.Local ? dateTime.ToUniversalTime() : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                return true;
            case DateTimeOffset offset:
                result = offset.UtcDateTime;
                return true;
            default:
                result = default;
                return false;
        }
    }

    private static bool TryGetLong(object value, out long result)
    {
        switch (value)
        {
            case long x:
                result = x;
                return true;
            case int x:
                result = x;
                return true;
            case short x:
                result = x;
                return true;
            case sbyte x:
                result = x;
                return true;
            case byte x:
                result = x;
                return true;
            case ushort x:
                result = x;
                return true;
            case uint x:
                result = x;
                return true;
            case ulong x when x <= long.MaxValue:
                result = (long)x;
                return true;
            default:
                result = 0;
                return false;
        }
    }

    private static bool TryGetDecimal(object value, out decimal result)
    {
        if (value is decimal exact)
        {
            result = exact;
            return true;
        }

        if (value is ulong big)
        {
            result = big;
            return true;
        }

        if (TryGetLong(value, out long whole))
        {
            result = whole;
            return true;
        }

        result = 0;
        return false;
    }
}
