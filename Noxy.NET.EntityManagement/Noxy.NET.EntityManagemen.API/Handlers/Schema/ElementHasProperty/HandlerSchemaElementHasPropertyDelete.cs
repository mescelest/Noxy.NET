using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.ElementHasProperty;

public class HandlerSchemaElementHasPropertyDelete(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaElementHasPropertyDelete, ResponseSchemaElementHasPropertyDelete>
{
    public async ValueTask<ResponseSchemaElementHasPropertyDelete> Handle(CommandSchemaElementHasPropertyDelete request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid result = await uow.Schema.DeleteSchemaElementHasProperty(request.ID);

        await uow.Commit();

        return new(result);
    }
}
