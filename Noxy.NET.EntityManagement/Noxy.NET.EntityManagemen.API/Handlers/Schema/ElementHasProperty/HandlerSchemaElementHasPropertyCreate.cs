using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.ElementHasProperty;

public class HandlerSchemaElementHasPropertyCreate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaElementHasPropertyCreate, ResponseSchemaElementHasPropertyCreate>
{
    public async ValueTask<ResponseSchemaElementHasPropertyCreate> Handle(CommandSchemaElementHasPropertyCreate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaElement entityElement = await uow.Schema.GetSchemaElementByID(command.SchemaElementID);
        EntitySchemaProperty entityProperty = await uow.Schema.GetSchemaPropertyByID(command.SchemaPropertyID);
        if (entityElement.SchemaID != entityProperty.SchemaID) throw new InvalidOperationException("SchemaElement and SchemaProperty must be in same schema.");

        EntitySchema schema = await uow.Schema.GetSchemaByID(entityElement.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveAddElementHasProperty, ParameterSystemConstants.SchemaDeactivatedAddElementHasProperty);

        EntitySchemaElementHasProperty result = await uow.Schema.CreateSchemaElementHasProperty(new()
        {
            EntityID = command.SchemaElementID,
            RelationID = command.SchemaPropertyID,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
