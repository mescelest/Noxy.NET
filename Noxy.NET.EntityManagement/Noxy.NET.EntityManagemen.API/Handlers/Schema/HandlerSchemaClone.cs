using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaClone(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaClone, ResponseSchemaClone>
{
    public async Task<ResponseSchemaClone> Handle(CommandSchemaClone request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema result = await uow.Schema.CloneSchema(request.ID);

        await uow.Commit();

        return new() { Value = result };
    }
}
