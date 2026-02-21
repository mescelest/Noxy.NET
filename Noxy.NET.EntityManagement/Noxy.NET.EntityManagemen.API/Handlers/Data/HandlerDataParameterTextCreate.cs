using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Data;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterTextCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandDataParameterTextCreate, ResponseDataParameterTextCreate>
{
    public async Task<ResponseDataParameterTextCreate> Handle(CommandDataParameterTextCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityDataParameterText result = await uow.Data.CreateTextParameter(request.SchemaIdentifier, request.Culture, request.Value, request.DateEffective);

        await uow.Commit();

        return new() { Value = result };
    }
}
