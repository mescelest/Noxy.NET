using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Context;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Context;

public class HandlerSchemaContextClone(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaContextClone, ResponseSchemaContextClone>
{
    public async ValueTask<ResponseSchemaContextClone> Handle(CommandSchemaContextClone request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaContext result = await uow.Schema.CloneSchemaContext(request.ID);

        await uow.Commit();

        return new() { Value = result };
    }
}
