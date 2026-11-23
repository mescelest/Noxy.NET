namespace Noxy.NET.EntityManagement.Application.Models;

// What a page of a context gets: the starting element, and the elements directly related to it whose kind is in the context, grouped by relation.
public sealed record ContextModel(string ContextIdentifier, ElementModel Element, IReadOnlyList<ContextRelationGroupModel> RelationGroupList);
