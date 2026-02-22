using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaContextCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaContextCreate, ResponseSchemaContextCreate>
{
    public async Task<ResponseSchemaContextCreate> Handle(CommandSchemaContextCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaContext result = await uow.Schema.Create(new EntitySchemaContext
        {
            SchemaIdentifier = request.SchemaIdentifier,
            Name = request.Name,
            Note = request.Note,
            TitleTextParameterID = request.TitleParameterTextID,
            DescriptionTextParameterID = request.DescriptionParameterTextID,
            SchemaID = request.SchemaID ?? Guid.Empty,
            Order = BaseEntityTemplate.DefaultOrder
        });

        return new() { Value = result };
    }
}
