using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaCreate(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaCreate, ResponseSchemaCreate>
{
    public async ValueTask<ResponseSchemaCreate> Handle(CommandSchemaCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema result = await uow.Schema.CreateSchema(new()
        {
            Name = request.Name,
            Note = request.Note,
            IsActive = false,
            TimeActivated = null,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
