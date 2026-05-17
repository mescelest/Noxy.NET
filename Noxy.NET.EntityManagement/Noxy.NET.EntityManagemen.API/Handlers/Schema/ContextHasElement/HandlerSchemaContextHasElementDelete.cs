using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.ContextHasElement;

public class HandlerSchemaContextHasElementDelete(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaContextHasElementDelete, ResponseSchemaContextHasElementDelete>
{
    public async ValueTask<ResponseSchemaContextHasElementDelete> Handle(CommandSchemaContextHasElementDelete request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid result = await uow.Schema.DeleteSchemaContextHasElement(request.ID);

        await uow.Commit();

        return new(result);
    }
}
