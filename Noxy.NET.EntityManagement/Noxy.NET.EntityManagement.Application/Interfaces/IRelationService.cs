using Noxy.NET.EntityManagement.Application.Models;

namespace Noxy.NET.EntityManagement.Application.Interfaces;

// Links elements to each other and reads the links, with everything checked against the definition.
// A bad request is not an exception of its own kind: the call throws a DataValidationException with every problem, and nothing is stored. A relation that is not defined is a mistake of the code, so it throws KeyNotFoundException.
public interface IRelationService
{
    // Makes a link that reads from one element to the other. Both must exist and be of the kinds the relation joins, the two must not have this relation already, and the limits of its cardinality must hold (OneToMany: an element it reads to can have only one; ManyToOne: an element it reads from can have only one; OneToOne: both).
    // The attributes are given as the values of an element are, and a required one must have a value. The link and its attributes are stored together, or not at all.
    Task<RelationModel> CreateRelation(string relationIdentifier, Guid fromElementID, Guid toElementID, IReadOnlyDictionary<string, object?> attributeMap, Guid userID, CancellationToken cancellationToken = default);

    Task<RelationModel?> FindRelation(Guid id, CancellationToken cancellationToken = default);

    // Every current link the element is part of, whichever way it reads: this is the element's links and its inverse links in one list. Oldest first. A kind of relation can be asked for.
    Task<IReadOnlyList<RelationModel>> GetRelationList(Guid elementID, string? relationIdentifier = null, CancellationToken cancellationToken = default);

    // Changes the attributes that are given and leaves the others as they are, as ChangeElement does for the values of an element. Null when there is no such relation, or it has ended.
    Task<RelationModel?> ChangeRelation(Guid id, IReadOnlyDictionary<string, object?> attributeMap, Guid userID, CancellationToken cancellationToken = default);

    // Ends the link and its attributes, which move to the history. The elements stay. False when there is no such relation. (Ending an element ends all of its links.)
    Task<bool> EndRelation(Guid id, Guid userID, CancellationToken cancellationToken = default);

    // The links that have ended, of an element in either direction, in the order they ended.
    Task<IReadOnlyList<RelationHistoryModel>> GetRelationHistory(Guid elementID, CancellationToken cancellationToken = default);

    // The attributes that have ended, of a link that is current or has ended, in the order they ended.
    Task<IReadOnlyList<PropertyHistoryModel>> GetAttributeHistory(Guid relationID, CancellationToken cancellationToken = default);
}
