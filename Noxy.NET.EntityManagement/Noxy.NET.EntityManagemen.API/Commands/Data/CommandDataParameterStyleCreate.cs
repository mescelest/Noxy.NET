using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Commands.Data;

public class CommandDataParameterStyleCreate(RequestDataParameterStyleCreate request) : IRequest<ResponseDataParameterStyleCreate>
{
    public string SchemaIdentifier { get; set; } = request.SchemaIdentifier;
    public string Value { get; set; } = request.Value;
    public DateTime? DateEffective { get; set; } = request.TimeEffective;
}
