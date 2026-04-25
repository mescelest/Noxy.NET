using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaUpdate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaUpdate, ResponseSchemaUpdate>
{
    public async Task<ResponseSchemaUpdate> Handle(CommandSchemaUpdate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema result = await uow.Schema.UpdateSchema(new()
        {
            ID = request.ID,
            Name = request.Name,
            Note = request.Note,
            IsActive = false,
            TimeActivated = null,
        });

        await uow.Commit();

        return new() { Value = result };
    }
}
