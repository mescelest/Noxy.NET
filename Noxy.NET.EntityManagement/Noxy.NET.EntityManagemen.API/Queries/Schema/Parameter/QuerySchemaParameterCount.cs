using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.Parameter;

public class QuerySchemaParameterCount(RequestSchemaParameterCount request) : IRequest<ResponseSchemaParameterCount>
{
    public Guid? SchemaID { get; } = request.SchemaID;
    public string? Search { get; } = request.Search;
    public bool? IsSystemDefined { get; } = request.IsSystemDefined;
    public bool? IsApprovalRequired { get; } = request.IsApprovalRequired;
    public IReadOnlySet<string>? ParameterType { get; } = request.ParameterType;
}
