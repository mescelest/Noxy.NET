using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Queries.Schema;

public class QuerySchemaParameterList(RequestSchemaParameterList request) : IRequest<ResponseSchemaParameterList>
{
    public string? Search { get; } = request.Search;
    public bool? IsSystemDefined { get; } = request.IsSystemDefined;
    public bool? IsApprovalRequired { get; } = request.IsApprovalRequired;
    public IReadOnlySet<string>? ParameterType { get; } = request.ParameterType;
    public int? PageNumber { get; } = request.PageNumber;
    public int? PageSize { get; } = request.PageSize;
}
