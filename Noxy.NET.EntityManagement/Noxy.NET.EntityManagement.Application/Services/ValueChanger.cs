using System.Diagnostics.CodeAnalysis;
using Noxy.NET.EntityManagement.Application.Enums;
using Noxy.NET.EntityManagement.Application.Models;
using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Models;
using Noxy.NET.EntityManagement.Domain.Entities;

namespace Noxy.NET.EntityManagement.Application.Services;

// What an element and a relation have in common: both own properties (the relation calls them attributes), given as values by the identifier of the property, and checked against the definition.
// Every problem is added to the list, not thrown, so the caller can report them all.
internal static class ValueChanger
{
    // The rows for a new owner. A value that is empty is left out, and a required property must have one.
    public static List<DataProperty> ForCreate(
        DefinitionRegistry definition,
        string ownerLabel,
        IReadOnlyList<string> propertyIdentifierList,
        IReadOnlyDictionary<string, object?> valueMap,
        Guid ownerID,
        Guid userID,
        DateTime now,
        List<DataError> errorList)
    {
        List<DataProperty> setList = [];

        foreach ((string propertyIdentifier, object? value) in valueMap)
        {
            if (!TryGetProperty(definition, ownerLabel, propertyIdentifierList, propertyIdentifier, errorList, out DefinitionProperty? property)) continue;
            if (PropertyValueConverter.IsEmpty(value)) continue;

            if (PropertyValueConverter.TryCreate(property, value!, ownerID, userID, now, out DataProperty? row, out DataError? error))
            {
                setList.Add(row);
            }
            else
            {
                errorList.Add(error);
            }
        }

        foreach (string propertyIdentifier in propertyIdentifierList)
        {
            // A value of the wrong type was already reported, so only a value that is missing or empty is reported here.
            if (definition.GetProperty(propertyIdentifier).IsRequired && PropertyValueConverter.IsEmpty(valueMap.GetValueOrDefault(propertyIdentifier)))
            {
                errorList.Add(RequiredError(propertyIdentifier));
            }
        }

        return setList;
    }

    // What to set and what to clear on an existing owner, from the values that were given. The properties that are not given stay as they are.
    // A value that is the same as the current one is not a change, and an empty value clears a property that has one. A required property cannot be cleared.
    public static (List<DataProperty> SetList, List<string> ClearList) ForChange(
        DefinitionRegistry definition,
        string ownerLabel,
        IReadOnlyList<string> propertyIdentifierList,
        IReadOnlyDictionary<string, object?> valueMap,
        IReadOnlyList<DataProperty> currentList,
        Guid ownerID,
        Guid userID,
        DateTime now,
        List<DataError> errorList)
    {
        List<DataProperty> setList = [];
        List<string> clearList = [];

        foreach ((string propertyIdentifier, object? value) in valueMap)
        {
            if (!TryGetProperty(definition, ownerLabel, propertyIdentifierList, propertyIdentifier, errorList, out DefinitionProperty? property)) continue;

            DataProperty? current = currentList.SingleOrDefault(x => x.PropertyIdentifier == propertyIdentifier);

            if (PropertyValueConverter.IsEmpty(value))
            {
                if (property.IsRequired)
                {
                    errorList.Add(RequiredError(propertyIdentifier));
                }
                else if (current is not null)
                {
                    clearList.Add(propertyIdentifier);
                }

                continue;
            }

            if (!PropertyValueConverter.TryCreate(property, value!, ownerID, userID, now, out DataProperty? row, out DataError? error))
            {
                errorList.Add(error);
                continue;
            }

            if (current is not null && PropertyValueConverter.AreEqual(current, row)) continue;

            setList.Add(row);
        }

        return (setList, clearList);
    }

    public static Dictionary<string, PropertyValueModel> ToValueMap(IEnumerable<DataProperty> propertyList)
    {
        return propertyList.ToDictionary(
            x => x.PropertyIdentifier,
            x => new PropertyValueModel(x.PropertyIdentifier, PropertyValueConverter.GetKind(x), PropertyValueConverter.GetValue(x), x.TimeCreated, x.CreatedByUserID));
    }

    // A property that is not defined, or that the owner does not have, is a mistake of whoever gave the values, which may be a request from outside, so it is reported with the other problems.
    private static bool TryGetProperty(DefinitionRegistry definition, string ownerLabel, IReadOnlyList<string> propertyIdentifierList, string propertyIdentifier, List<DataError> errorList, [NotNullWhen(true)] out DefinitionProperty? property)
    {
        if (propertyIdentifierList.Contains(propertyIdentifier) && definition.TryGetProperty(propertyIdentifier, out property)) return true;

        errorList.Add(new(propertyIdentifier, DataErrorEnum.UnknownProperty, $"{ownerLabel} has no property '{propertyIdentifier}'."));
        property = null;
        return false;
    }

    private static DataError RequiredError(string propertyIdentifier)
    {
        return new(propertyIdentifier, DataErrorEnum.Required, $"The property '{propertyIdentifier}' is required.");
    }
}
