using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaDelete(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaDelete, ResponseSchemaDelete>
{
    public async Task<ResponseSchemaDelete> Handle(CommandSchemaDelete request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid result = await uow.Schema.Delete(request.ID);

        await uow.Commit();

        return new() { Value = result };
    }
}
