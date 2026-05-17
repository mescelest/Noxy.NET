namespace Noxy.NET.EntityManagement.Domain.Models.Filters;

public class FilterSchemaElementList
{
    public Guid SchemaID { get; init; }
    public string? Search { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
}
