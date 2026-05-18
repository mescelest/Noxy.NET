using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyStringUpdate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaPropertyStringUpdate, ResponseSchemaPropertyStringUpdate>
{
    public async ValueTask<ResponseSchemaPropertyStringUpdate> Handle(CommandSchemaPropertyStringUpdate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaProperty.Discriminator discriminator = await uow.Schema.GetSchemaPropertyByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(discriminator.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveEditParameter, ParameterSystemConstants.SchemaDeactivatedEditParameter);
        if (discriminator.GetValue() is not EntitySchemaPropertyString entity) throw new InvalidOperationException("Property is not of type String");

        entity.SchemaIdentifier = command.SchemaIdentifier;
        entity.Name = command.Name;
        entity.Note = command.Note;
        entity.Weight = command.Weight ?? BaseEntity.DefaultWeight;
        entity.TitleParameterTextID = command.TitleParameterTextID;
        entity.DescriptionParameterTextID = command.DescriptionParameterTextID;
        uow.Schema.UpdateSchemaPropertyString(entity);

        await uow.Commit();

        return new(entity.ID);
    }
}
