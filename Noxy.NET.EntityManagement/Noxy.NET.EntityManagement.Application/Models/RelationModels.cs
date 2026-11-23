namespace Noxy.NET.EntityManagement.Application.Models;

// A link between two elements with its current attributes, which are the properties of the link itself (a role, a note), as the values of an element are.
// It reads from one element to the other. Seen from the element it reads to, it is the same relation: that is the inverse view, and the identifiers say which end an element is.
public sealed record RelationModel(Guid ID, string RelationIdentifier, Guid FromElementID, Guid ToElementID, DateTime TimeCreated, Guid CreatedByUserID, IReadOnlyDictionary<string, PropertyValueModel> AttributeMap);

// A link that has ended, and when and by whom.
public sealed record RelationHistoryModel(Guid ID, string RelationIdentifier, Guid FromElementID, Guid ToElementID, DateTime TimeCreated, Guid CreatedByUserID, DateTime TimeEnded, Guid EndedByUserID);
