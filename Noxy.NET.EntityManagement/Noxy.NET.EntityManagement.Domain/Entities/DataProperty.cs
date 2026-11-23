using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Entities;

// One value of one element (or, later, of one relation), held in the type the schema gave the property.
// There is one table for each type, so the value is stored strongly typed.
public abstract class DataProperty : BaseData
{
    // The element (or relation) the value belongs to. It is not a foreign key: a row that has moved to the history must not be held back by, or deleted with, the rows that point at it.
    public required Guid OwnerID { get; init; }

    // The identifier of the property in the schema.
    public required string PropertyIdentifier { get; init; }

    public abstract HistoryProperty ToHistory(Guid endedByUserID, DateTime timeEnded);
}

public abstract class DataProperty<TValue, THistory> : DataProperty where THistory : HistoryProperty<TValue>, new()
{
    public required TValue Value { get; init; }

    public override HistoryProperty ToHistory(Guid endedByUserID, DateTime timeEnded)
    {
        return new THistory
        {
            ID = ID,
            TimeCreated = TimeCreated,
            CreatedByUserID = CreatedByUserID,
            OwnerID = OwnerID,
            PropertyIdentifier = PropertyIdentifier,
            Value = Value,
            TimeEnded = timeEnded,
            EndedByUserID = endedByUserID
        };
    }
}

public abstract class HistoryProperty : BaseHistory
{
    public Guid OwnerID { get; init; }
    public string PropertyIdentifier { get; init; } = string.Empty;
}

public abstract class HistoryProperty<TValue> : HistoryProperty
{
    public TValue Value { get; init; } = default!;
}
