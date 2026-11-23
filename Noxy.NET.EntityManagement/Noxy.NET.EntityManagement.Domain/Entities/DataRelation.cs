using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Entities;

// A link between two elements, of one kind of relation in the schema ("person-involved-in-case"). It reads from the first element to the second; the same row is the inverse view from the second.
// What the link itself has (a role, a note) is stored as ordinary properties, with the ID of the relation as their owner.
// Like the owner of a property, the elements are not foreign keys: a row that has moved to the history must not be held back by the rows that point at it.
public sealed class DataRelation : BaseData
{
    // The identifier of the relation in the schema.
    public required string RelationIdentifier { get; init; }

    public required Guid FromElementID { get; init; }
    public required Guid ToElementID { get; init; }

    public HistoryRelation ToHistory(Guid endedByUserID, DateTime timeEnded)
    {
        return new()
        {
            ID = ID,
            TimeCreated = TimeCreated,
            CreatedByUserID = CreatedByUserID,
            RelationIdentifier = RelationIdentifier,
            FromElementID = FromElementID,
            ToElementID = ToElementID,
            TimeEnded = timeEnded,
            EndedByUserID = endedByUserID
        };
    }
}

public sealed class HistoryRelation : BaseHistory
{
    public string RelationIdentifier { get; init; } = string.Empty;
    public Guid FromElementID { get; init; }
    public Guid ToElementID { get; init; }
}
