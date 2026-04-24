namespace Noxy.NET.EntityManagement.Application.Models;

public class FilterSchemaElementList
{
    public Guid SchemaID { get; init; }
    public string? Search { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
}
