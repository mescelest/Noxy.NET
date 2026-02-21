using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Data;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterStyleCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandDataParameterStyleCreate, ResponseDataParameterStyleCreate>
{
    public async Task<ResponseDataParameterStyleCreate> Handle(CommandDataParameterStyleCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityDataParameterStyle result = await uow.Data.CreateStyleParameter(request.SchemaIdentifier, request.Value, request.DateEffective);

        await uow.Commit();

        return new() { Value = result };
    }
}
