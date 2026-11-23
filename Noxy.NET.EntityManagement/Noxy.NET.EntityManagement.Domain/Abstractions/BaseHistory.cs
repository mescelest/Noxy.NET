namespace Noxy.NET.EntityManagement.Domain.Abstractions;

// A row that has ended. It keeps the ID and everything the row had, and adds when it ended and who ended it.
// The members are not required, so the history classes can be created by the generic code that moves a row to the history.
public abstract class BaseHistory
{
    public Guid ID { get; init; }
    public DateTime TimeCreated { get; init; }
    public Guid CreatedByUserID { get; init; }

    public DateTime TimeEnded { get; init; }
    public Guid EndedByUserID { get; init; }
}
