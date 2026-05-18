using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public sealed class HandlerSchemaActivate(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaActivate, ResponseSchemaActivate>
{
    public async ValueTask<ResponseSchemaActivate> Handle(CommandSchemaActivate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema entity = await uow.Schema.GetSchemaByID(command.ID);
        if (entity.IsActive && entity.TimeActivated.HasValue) return new(entity.TimeActivated.Value);

        entity.IsActive = true;
        entity.TimeActivated = DateTime.UtcNow;
        uow.Schema.UpdateSchema(entity);

        await uow.Schema.DeactivateSchemaExcept(entity.ID);
        await uow.Commit();

        return new(entity.TimeActivated.Value);
    }
}
