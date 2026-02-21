using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QuerySchemaParameterList(RequestSchemaParameterList request) : IRequest<ResponseSchemaParameterList>
{
    public string? Search { get; } = request.Search;
    public bool? IsSystemDefined { get; } = request.IsSystemDefined;
    public bool? IsApprovalRequired { get; } = request.IsApprovalRequired;
    public IReadOnlyList<string>? ParameterType { get; } = request.ParameterType;
    public int? PageNumber { get; } = request.PageNumber;
    public int? PageSize { get; } = request.PageSize;
}
