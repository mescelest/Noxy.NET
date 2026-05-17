using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterDelete(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaParameterDelete, ResponseSchemaParameterDelete>
{
    public async ValueTask<ResponseSchemaParameterDelete> Handle(CommandSchemaParameterDelete request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid result = await uow.Schema.DeleteSchemaParameter(request.ID);

        await uow.Commit();

        return new(result);
    }
}
