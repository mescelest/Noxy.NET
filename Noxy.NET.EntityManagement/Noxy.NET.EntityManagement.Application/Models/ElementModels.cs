using Noxy.NET.EntityManagement.Application.Enums;
using Noxy.NET.EntityManagement.Definitions.Enums;

namespace Noxy.NET.EntityManagement.Application.Models;

// An element with its current values. The value of a property is of the type of its kind: bool, DateTime (UTC), decimal, long, or string (for a string and for the reference of an image).
public sealed record ElementModel(Guid ID, string ElementIdentifier, DateTime TimeCreated, Guid CreatedByUserID, IReadOnlyDictionary<string, PropertyValueModel> PropertyMap);
public sealed record PropertyValueModel(string PropertyIdentifier, PropertyKindEnum Kind, object Value, DateTime TimeCreated, Guid CreatedByUserID);

// A value that has ended, and when and by whom.
public sealed record PropertyHistoryModel(string PropertyIdentifier, PropertyKindEnum Kind, object Value, DateTime TimeCreated, Guid CreatedByUserID, DateTime TimeEnded, Guid EndedByUserID);

// One page of the elements of a kind, and how many there are in all.
public sealed record ElementPageModel(IReadOnlyList<ElementModel> ElementList, int TotalCount);

// One thing wrong with the values that were given. The property is null when it is not about one property.
public sealed record DataError(string? PropertyIdentifier, DataErrorEnum Code, string Message);
