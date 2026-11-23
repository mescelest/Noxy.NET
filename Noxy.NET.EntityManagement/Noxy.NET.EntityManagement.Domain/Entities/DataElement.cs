using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Entities;

// One thing (a person, a case). It only says which element of the schema it is; everything about it is in its properties.
public sealed class DataElement : BaseData
{
    public required string SchemaIdentifier { get; init; }

    public HistoryElement ToHistory(Guid endedByUserID, DateTime timeEnded)
    {
        return new()
        {
            ID = ID,
            TimeCreated = TimeCreated,
            CreatedByUserID = CreatedByUserID,
            SchemaIdentifier = SchemaIdentifier,
            TimeEnded = timeEnded,
            EndedByUserID = endedByUserID
        };
    }
}

public sealed class HistoryElement : BaseHistory
{
    public string SchemaIdentifier { get; init; } = string.Empty;
}
