using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryDataParameterSystemCreate(RequestDataParameterSystemCreate request) : IRequest<ResponseDataParameterSystemCreate>
{
    public string SchemaIdentifier { get; set; } = request.SchemaIdentifier;
    public string Value { get; set; } = request.Value;
    public DateTime? DateEffective { get; set; } = request.DateEffective;
}
