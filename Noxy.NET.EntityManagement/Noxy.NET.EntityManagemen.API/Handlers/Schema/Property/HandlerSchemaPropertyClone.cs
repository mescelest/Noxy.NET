using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyClone(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaPropertyClone, ResponseSchemaPropertyClone>
{
    public async ValueTask<ResponseSchemaPropertyClone> Handle(CommandSchemaPropertyClone request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaProperty.Discriminator result = await uow.Schema.CloneSchemaProperty(request.ID);

        await uow.Commit();

        return new(result.ID);
    }
}
