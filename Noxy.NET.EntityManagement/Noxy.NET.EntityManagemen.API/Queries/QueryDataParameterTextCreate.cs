using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryDataParameterTextCreate(RequestDataParameterTextCreate request) : IRequest<ResponseDataParameterTextCreate>
{
    public string SchemaIdentifier { get; set; } = request.SchemaIdentifier;
    public string Value { get; set; } = request.Value;
    public DateTime? DateEffective { get; set; } = request.DateEffective;
}
