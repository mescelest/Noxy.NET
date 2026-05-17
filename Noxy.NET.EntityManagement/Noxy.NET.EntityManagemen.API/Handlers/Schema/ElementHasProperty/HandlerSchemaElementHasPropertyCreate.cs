using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.ElementHasProperty;

public class HandlerSchemaElementHasPropertyCreate(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaElementHasPropertyCreate, ResponseSchemaElementHasPropertyCreate>
{
    public async ValueTask<ResponseSchemaElementHasPropertyCreate> Handle(CommandSchemaElementHasPropertyCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaElementHasProperty result = await uow.Schema.CreateSchemaElementHasProperty(new()
        {
            RelationID = request.SchemaPropertyID,
            EntityID = request.SchemaElementID
        });

        await uow.Commit();

        return new() { Value = result };
    }
}
