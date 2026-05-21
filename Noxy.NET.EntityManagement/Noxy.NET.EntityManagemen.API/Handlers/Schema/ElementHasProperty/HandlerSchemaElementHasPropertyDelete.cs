using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.ElementHasProperty;

public class HandlerSchemaElementHasPropertyDelete(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaElementHasPropertyDelete, ResponseSchemaElementHasPropertyDelete>
{
    public async ValueTask<ResponseSchemaElementHasPropertyDelete> Handle(CommandSchemaElementHasPropertyDelete command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaElementHasProperty entity = await uow.Schema.GetSchemaElementHasPropertyByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(entity.Entity!.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveElementHasPropertyDelete, ParameterSystemConstants.SchemaDeactivatedElementHasPropertyDelete);

        uow.Schema.DeleteSchemaElementHasProperty(entity);

        await uow.Commit();

        return new(entity.ID);
    }
}
