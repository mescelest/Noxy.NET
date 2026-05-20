using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyDelete(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaPropertyDelete, ResponseSchemaPropertyDelete>
{
    public async ValueTask<ResponseSchemaPropertyDelete> Handle(CommandSchemaPropertyDelete command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaProperty entity = await uow.Schema.GetSchemaPropertyByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(entity.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveDeleteProperty, ParameterSystemConstants.SchemaDeactivatedDeleteProperty);

        uow.Schema.DeleteSchemaProperty(entity);

        await uow.Commit();

        return new(entity.ID);
    }
}
