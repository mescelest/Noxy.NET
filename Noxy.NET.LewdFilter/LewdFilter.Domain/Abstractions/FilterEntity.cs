namespace LewdFilter.Domain;

public abstract class FilterEntity
{
    public Guid ID { get; set; } = Guid.CreateVersion7();
}
