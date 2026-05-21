using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.ContextHasElement;

public class HandlerSchemaContextHasElementCreate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaContextHasElementCreate, ResponseSchemaContextHasElementCreate>
{
    public async ValueTask<ResponseSchemaContextHasElementCreate> Handle(CommandSchemaContextHasElementCreate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaContext entityContext = await uow.Schema.GetSchemaContextByID(command.SchemaContextID);
        EntitySchemaElement entityElement = await uow.Schema.GetSchemaElementByID(command.SchemaElementID);
        if (entityContext.SchemaID != entityElement.SchemaID) throw new InvalidOperationException("SchemaContext and SchemaElement must be in same schema.");

        EntitySchema schema = await uow.Schema.GetSchemaByID(entityContext.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveContextHasElementAdd, ParameterSystemConstants.SchemaDeactivatedContextHasElementAdd);

        EntitySchemaContextHasElement result = await uow.Schema.CreateSchemaContextHasElement(new()
        {
            EntityID = command.SchemaContextID,
            RelationID = command.SchemaElementID,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
