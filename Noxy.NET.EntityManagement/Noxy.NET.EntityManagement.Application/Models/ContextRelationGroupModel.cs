using Noxy.NET.EntityManagement.Application.Enums;

namespace Noxy.NET.EntityManagement.Application.Models;

// The elements related to the starting element by one relation, read one way. The group is there when the definition allows it, also when nothing is related yet, so a page can show an empty list.
public sealed record ContextRelationGroupModel(string RelationIdentifier, RelationDirectionEnum Direction, string ElementIdentifier, IReadOnlyList<ContextRelatedModel> RelatedList);
