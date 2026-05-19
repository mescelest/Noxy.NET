using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Data;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterStyleCreate(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandDataParameterStyleCreate, ResponseDataParameterStyleCreate>
{
    public async ValueTask<ResponseDataParameterStyleCreate> Handle(CommandDataParameterStyleCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        // TODO: move logic from repo to handler and also update the parameter service 

        Guid id = await uow.Schema.GetCurrentSchemaID();
        EntityDataParameterStyle result = await uow.Data.CreateParameterStyle(id, request.SchemaIdentifier, request.Value, request.DateEffective);

        await uow.Commit();

        return new() { Value = result };
    }
}
