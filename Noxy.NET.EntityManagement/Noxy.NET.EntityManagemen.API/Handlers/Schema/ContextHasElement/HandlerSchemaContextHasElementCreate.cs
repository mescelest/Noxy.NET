using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.ContextHasElement;

public class HandlerSchemaContextHasElementCreate(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaContextHasElementCreate, ResponseSchemaContextHasElementCreate>
{
    public async ValueTask<ResponseSchemaContextHasElementCreate> Handle(CommandSchemaContextHasElementCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaContextHasElement result = await uow.Schema.CreateSchemaContextHasElement(new()
        {
            RelationID = request.SchemaElementID,
            EntityID = request.SchemaContextID
        });

        await uow.Commit();

        return new() { Value = result };
    }
}
