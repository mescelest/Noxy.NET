using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaDelete(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaDelete, ResponseSchemaDelete>
{
    public async ValueTask<ResponseSchemaDelete> Handle(CommandSchemaDelete request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid result = await uow.Schema.DeleteSchema(request.ID);

        await uow.Commit();

        return new(result);
    }
}
