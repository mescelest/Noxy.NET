using Mediator;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.ElementHasProperty;

public class CommandSchemaElementHasPropertyCreate(RequestSchemaElementHasPropertyCreate request) : ICommand<ResponseSchemaElementHasPropertyCreate>
{
    public Guid SchemaPropertyID { get; } = request.SchemaPropertyID;
    public Guid SchemaElementID { get; } = request.SchemaElementID;
}
