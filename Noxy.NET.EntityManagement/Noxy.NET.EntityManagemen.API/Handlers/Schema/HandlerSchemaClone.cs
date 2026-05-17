using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public sealed class HandlerSchemaClone(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaClone, ResponseSchemaClone>
{
    public async ValueTask<ResponseSchemaClone> Handle(CommandSchemaClone command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema result = await uow.Schema.CloneSchema(command.ID);

        await uow.Commit();

        return new(result.ID);
    }
}
