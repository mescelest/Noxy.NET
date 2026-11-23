namespace Noxy.NET.EntityManagement.Domain.Abstractions;

// What every row of current data has. A row is never changed: a change ends the row (it moves to the history) and creates a new one.
public abstract class BaseData
{
    // The same as the longest identifier the schema allows (a test keeps the two equal).
    public const int IdentifierMaxLength = 100;

    // For free text of any size. It only gives the column a bound, which is far above anything entered.
    public const int TextMaxLength = 1_000_000;

    // Who made a row that no user made.
    public static readonly Guid SystemUserID = Guid.Empty;

    public Guid ID { get; init; } = CreateID();
    public DateTime TimeCreated { get; init; } = DateTime.UtcNow;
    public required Guid CreatedByUserID { get; init; }

    public static Guid CreateID()
    {
        return Guid.CreateVersion7();
    }
}
