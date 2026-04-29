using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema.Context;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Element;

public class HandlerSchemaContextClone(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaContextClone, ResponseSchemaContextClone>
{
    public async Task<ResponseSchemaContextClone> Handle(CommandSchemaContextClone request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaContext result = await uow.Schema.CloneSchemaContext(request.ID);

        await uow.Commit();

        return new() { Value = result };
    }
}
