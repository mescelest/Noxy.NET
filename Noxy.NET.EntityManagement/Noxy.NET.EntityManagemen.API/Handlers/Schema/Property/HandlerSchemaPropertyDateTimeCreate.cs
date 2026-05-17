using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyDateTimeCreate(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaPropertyDateTimeCreate, ResponseSchemaPropertyDateTimeCreate>
{
    public async ValueTask<ResponseSchemaPropertyDateTimeCreate> Handle(CommandSchemaPropertyDateTimeCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaPropertyDateTime result = await uow.Schema.CreateSchemaPropertyDateTime(new()
        {
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            SchemaIdentifier = request.SchemaIdentifier,
            Name = request.Name,
            Note = request.Note,
            Type = request.Type,
            Weight = request.Weight ?? BaseEntity.DefaultWeight,
            TitleTextParameterID = request.TitleParameterTextID,
            DescriptionTextParameterID = request.DescriptionParameterTextID,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
