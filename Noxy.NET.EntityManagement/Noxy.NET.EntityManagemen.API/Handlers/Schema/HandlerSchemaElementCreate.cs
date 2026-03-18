using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaElementCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaElementCreate, ResponseSchemaElementCreate>
{
    public async Task<ResponseSchemaElementCreate> Handle(CommandSchemaElementCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaElement result = await uow.Schema.Create(new EntitySchemaElement
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
