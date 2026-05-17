using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Element;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Element;

public class HandlerSchemaElementClone(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaElementClone, ResponseSchemaElementClone>
{
    public async ValueTask<ResponseSchemaElementClone> Handle(CommandSchemaElementClone request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaElement result = await uow.Schema.CloneSchemaElement(request.ID);

        await uow.Commit();

        return new(result.ID);
    }
}
