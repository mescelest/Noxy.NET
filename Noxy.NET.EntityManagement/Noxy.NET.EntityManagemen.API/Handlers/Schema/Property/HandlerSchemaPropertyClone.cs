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

public class HandlerSchemaPropertyClone(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaPropertyClone, ResponseSchemaPropertyClone>
{
    public async ValueTask<ResponseSchemaPropertyClone> Handle(CommandSchemaPropertyClone command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaProperty.Discriminator entity = await uow.Schema.GetSchemaPropertyByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(entity.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveCloneProperty, ParameterSystemConstants.SchemaDeactivatedCloneProperty);

        EntitySchemaProperty value = entity.GetValue();
        value.ID = BaseEntity.CreateID();
        value.SchemaIdentifier = BaseEntity.CreateTemporarySchemaIdentifier();
        value.TimeCreated = DateTime.UtcNow;

        EntitySchemaProperty result = value switch
        {
            EntitySchemaPropertyBoolean v => await uow.Schema.CreateSchemaPropertyBoolean(v),
            EntitySchemaPropertyDateTime v => await uow.Schema.CreateSchemaPropertyDateTime(v),
            EntitySchemaPropertyDecimal v => await uow.Schema.CreateSchemaPropertyDecimal(v),
            EntitySchemaPropertyImage v => await uow.Schema.CreateSchemaPropertyImage(v),
            EntitySchemaPropertyInteger v => await uow.Schema.CreateSchemaPropertyInteger(v),
            EntitySchemaPropertyString v => await uow.Schema.CreateSchemaPropertyString(v),
            _ => throw new InvalidOperationException(),
        };

        await uow.Commit();

        return new(result.ID);
    }
}
