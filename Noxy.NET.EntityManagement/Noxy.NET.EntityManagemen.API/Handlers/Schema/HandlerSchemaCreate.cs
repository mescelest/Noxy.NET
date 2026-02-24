using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaCreate, ResponseSchemaCreate>
{
    public async Task<ResponseSchemaCreate> Handle(CommandSchemaCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema result = await uow.Schema.Create(new EntitySchema
        {
            Name = request.Name,
            Note = request.Note,
            Order = BaseEntityTemplate.DefaultOrder,
            IsActive = false,
            TimeActivated = null,
        });

        return new() { Value = result };
    }
}
