using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QuerySchemaParameterList(RequestSchemaParameterList request) : IRequest<ResponseSchemaParameterList>
{
    public string? Search { get; set; } = request.Search;
    public bool? IsSystemDefined { get; set; } = request.IsSystemDefined;
    public bool? IsApprovalRequired { get; set; } = request.IsApprovalRequired;
}
