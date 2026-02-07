using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QuerySchemaParameterList(RequestSchemaParameterList request) : IRequest<ResponseSchemaParameterList>
{
    public string? Search { get; } = request.Search;
    public bool? IsSystemDefined { get; } = request.IsSystemDefined;
    public bool? IsApprovalRequired { get; } = request.IsApprovalRequired;
    public int? PageNumber { get; } = request.PageNumber;
    public int? PageSize { get; } = request.PageSize;
}
