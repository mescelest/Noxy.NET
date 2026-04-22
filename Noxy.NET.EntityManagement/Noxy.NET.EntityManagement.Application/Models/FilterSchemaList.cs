namespace Noxy.NET.EntityManagement.Application.Models;

public class FilterSchemaList
{
    public string? Search { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
}
