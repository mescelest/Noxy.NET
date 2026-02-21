namespace Noxy.NET.EntityManagement.Application.Models;

public class FilterSchemaParameterList
{
    public string? Search { get; init; }
    public bool? IsSystemDefined { get; init; }
    public bool? IsApprovalRequired { get; init; }
    public IReadOnlyList<string>? ParameterType { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
}
