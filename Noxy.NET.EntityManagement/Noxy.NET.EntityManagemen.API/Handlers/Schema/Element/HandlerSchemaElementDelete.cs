using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Element;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Element;

public class HandlerSchemaElementDelete(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaElementDelete, ResponseSchemaElementDelete>
{
    public async ValueTask<ResponseSchemaElementDelete> Handle(CommandSchemaElementDelete request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid result = await uow.Schema.DeleteSchemaElement(request.ID);

        await uow.Commit();

        return new() { Value = result };
    }
}
