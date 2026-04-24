using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Data;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterSystemCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandDataParameterSystemCreate, ResponseDataParameterSystemCreate>
{
    public async Task<ResponseDataParameterSystemCreate> Handle(CommandDataParameterSystemCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid id = await uow.Schema.GetCurrentSchemaID();
        EntityDataParameterSystem result = await uow.Data.CreateSystemParameter(id, request.SchemaIdentifier, request.Value, request.DateEffective);

        await uow.Commit();

        return new() { Value = result };
    }
}
