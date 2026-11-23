namespace Noxy.NET.EntityManagement.Application.Models;

// One related element, with the link to it: the id of the relation and its own attributes (a role, a note).
public sealed record ContextRelatedModel(Guid RelationID, DateTime RelationTimeCreated, IReadOnlyDictionary<string, PropertyValueModel> AttributeMap, ElementModel Element);
