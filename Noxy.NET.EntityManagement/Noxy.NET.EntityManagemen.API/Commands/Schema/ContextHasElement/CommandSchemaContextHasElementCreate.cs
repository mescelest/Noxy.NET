using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.ContextHasElement;

public class CommandSchemaContextHasElementCreate(RequestSchemaContextHasElementCreate request) : IRequest<ResponseSchemaContextHasElementCreate>
{
    public Guid SchemaContextID { get; } = request.SchemaContextID;
    public Guid SchemaElementID { get; } = request.SchemaElementID;
}
