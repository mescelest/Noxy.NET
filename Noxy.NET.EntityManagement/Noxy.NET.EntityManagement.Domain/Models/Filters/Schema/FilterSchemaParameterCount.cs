namespace Noxy.NET.EntityManagement.Domain.Models.Filters.Schema;

public class FilterSchemaParameterCount
{
    public Guid SchemaID { get; init; }
    public string? Search { get; init; }
    public bool? IsSystemDefined { get; init; }
    public bool? IsApprovalRequired { get; init; }
    public IReadOnlySet<string>? ParameterType { get; init; }
}
