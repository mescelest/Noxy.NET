namespace Noxy.NET.EntityManagement.Domain.Models.Filters.Schema;

public class FilterSchemaPropertyCount
{
    public Guid SchemaID { get; init; }
    public string? Search { get; init; }
    public IReadOnlySet<string>? PropertyType { get; init; }
}
