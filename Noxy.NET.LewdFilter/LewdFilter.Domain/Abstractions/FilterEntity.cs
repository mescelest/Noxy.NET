namespace LewdFilter.Domain.Abstractions;

public abstract record FilterEntity
{
    public Guid ID { get; set; } = Guid.CreateVersion7();
}
