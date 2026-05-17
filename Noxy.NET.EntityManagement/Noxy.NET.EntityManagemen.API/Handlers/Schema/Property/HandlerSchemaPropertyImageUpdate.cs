using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyImageUpdate(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaPropertyImageUpdate, ResponseSchemaPropertyImageUpdate>
{
    public async ValueTask<ResponseSchemaPropertyImageUpdate> Handle(CommandSchemaPropertyImageUpdate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaPropertyImage result = await uow.Schema.UpdateSchemaPropertyImage(new()
        {
            ID = request.ID,
            SchemaID = Guid.Empty,
            SchemaIdentifier = request.SchemaIdentifier,
            Name = request.Name,
            Note = request.Note,
            Weight = request.Weight ?? BaseEntity.DefaultWeight,
            AllowedExtensions = request.AllowedExtensions,
            TitleTextParameterID = request.TitleParameterTextID,
            DescriptionTextParameterID = request.DescriptionParameterTextID,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
