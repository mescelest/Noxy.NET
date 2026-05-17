using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyDelete(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaPropertyDelete, ResponseSchemaPropertyDelete>
{
    public async ValueTask<ResponseSchemaPropertyDelete> Handle(CommandSchemaPropertyDelete request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid result = await uow.Schema.DeleteSchemaProperty(request.ID);

        await uow.Commit();

        return new(result);
    }
}
