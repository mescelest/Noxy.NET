using Mediator;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.ContextHasElement;

public class CommandSchemaContextHasElementCreate(RequestSchemaContextHasElementCreate request) : ICommand<ResponseSchemaContextHasElementCreate>
{
    public Guid SchemaContextID { get; } = request.SchemaContextID;
    public Guid SchemaElementID { get; } = request.SchemaElementID;
}
