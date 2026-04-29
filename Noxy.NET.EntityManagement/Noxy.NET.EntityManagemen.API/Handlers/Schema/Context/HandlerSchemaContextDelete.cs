using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema.Context;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Element;

public class HandlerSchemaContextDelete(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaContextDelete, ResponseSchemaContextDelete>
{
    public async Task<ResponseSchemaContextDelete> Handle(CommandSchemaContextDelete request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid result = await uow.Schema.DeleteSchemaContext(request.ID);

        await uow.Commit();

        return new() { Value = result };
    }
}
