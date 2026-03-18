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
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            SchemaIdentifier = request.SchemaIdentifier,
            Description = new(request.Name, request.Note),
            Presentation = new(request.TitleParameterTextID, request.DescriptionParameterTextID),
            Ordering = new(BaseEntity.FeatureOrdering.DefaultOrder),
        });

        return new() { Value = result };
    }
}
