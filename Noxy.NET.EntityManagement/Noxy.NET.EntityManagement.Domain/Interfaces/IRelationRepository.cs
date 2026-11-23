using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Domain.Models;

namespace Noxy.NET.EntityManagement.Domain.Interfaces;

// Stores the links between elements. Like the rest of the data, nothing is changed in place: a link that ends moves to the history. What a link itself has (its attributes) is stored as properties, owned by the relation, and is read with IDataRepository.GetPropertyList.
// Every call that writes does all of its work or none of it. The repository does not check the data against the schema, except for the limits it is given; that is for the layer above.
public interface IRelationRepository
{
    // The relation and its attributes, all or nothing, and only if the checks hold. They are done in the same transaction as the insert, so two requests at once cannot both pass a limit.
    // maxLinksOfFrom is how many current relations of this kind the element it reads from may have in all, counting this one (null: no limit), and maxLinksOfTo the same for the element it reads to.
    // The attributes must be of the relation and must not repeat a property.
    Task<CreateRelationResult> CreateRelation(DataRelation relation, IReadOnlyList<DataProperty> attributeList, int? maxLinksOfFrom, int? maxLinksOfTo, CancellationToken cancellationToken = default);

    Task<DataRelation?> FindRelation(Guid id, CancellationToken cancellationToken = default);

    // Every current relation the element is part of, whichever way it reads: the element is the one it reads from, or the one it reads to. Oldest first. A kind of relation can be asked for.
    Task<List<DataRelation>> GetRelationList(Guid elementID, string? relationIdentifier = null, CancellationToken cancellationToken = default);

    // Sets and clears attributes of a relation in one step, all or nothing, and only while the relation exists. False when it does not. The rules are those of IDataRepository.ChangeElementProperties.
    Task<bool> ChangeRelationAttributes(Guid relationID, IReadOnlyList<DataProperty> setList, IReadOnlyList<string> clearList, Guid endedByUserID, CancellationToken cancellationToken = default);

    // Ends the relation and its attributes: their rows move to the history. False when there is no such relation. (Ending an element ends all of its relations too.)
    Task<bool> EndRelation(Guid id, Guid endedByUserID, CancellationToken cancellationToken = default);

    // The relations that have ended, of an element in either direction, in the order they ended.
    Task<List<HistoryRelation>> GetRelationHistoryList(Guid elementID, CancellationToken cancellationToken = default);
}
