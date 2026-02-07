namespace Noxy.NET.EntityManagement.Persistence.Models;

public class FilterSchemaParameterList
{
    public string? Search { get; init; }
    public bool? IsSystemDefined { get; init; }
    public bool? IsApprovalRequired { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
}
