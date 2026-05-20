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

public class HandlerSchemaPropertyDecimalUpdate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaPropertyDecimalUpdate, ResponseSchemaPropertyDecimalUpdate>
{
    public async ValueTask<ResponseSchemaPropertyDecimalUpdate> Handle(CommandSchemaPropertyDecimalUpdate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaProperty discriminator = await uow.Schema.GetSchemaPropertyByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(discriminator.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveEditProperty, ParameterSystemConstants.SchemaDeactivatedEditProperty);
        if (discriminator is not EntitySchemaPropertyDecimal entity) throw new InvalidOperationException("Property is not of type Decimal");

        entity.SchemaIdentifier = command.SchemaIdentifier;
        entity.Name = command.Name;
        entity.Note = command.Note;
        entity.Weight = command.Weight ?? BaseEntity.DefaultWeight;
        entity.TitleParameterTextID = command.TitleParameterTextID;
        entity.DescriptionParameterTextID = command.DescriptionParameterTextID;
        uow.Schema.UpdateSchemaPropertyDecimal(entity);

        await uow.Commit();

        return new(entity.ID);
    }
}
