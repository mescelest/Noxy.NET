using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterSystemUpdate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaParameterSystemUpdate, ResponseSchemaParameterSystemUpdate>
{
    public async ValueTask<ResponseSchemaParameterSystemUpdate> Handle(CommandSchemaParameterSystemUpdate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaParameter.Discriminator discriminator = await uow.Schema.GetSchemaParameterByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(discriminator.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveEditParameter, ParameterSystemConstants.SchemaDeactivatedEditParameter);
        if (discriminator.GetValue() is not EntitySchemaParameterSystem entity) throw new InvalidOperationException("Parameter is not of type System");

        entity.SchemaIdentifier = command.SchemaIdentifier;
        entity.Name = command.Name;
        entity.Note = command.Note;
        entity.Type = command.Type;
        entity.IsApprovalRequired = command.IsApprovalRequired;
        entity.IsSystemDefined = command.IsSystemDefined;
        uow.Schema.UpdateSchemaParameterSystem(entity);

        await uow.Commit();

        return new(entity.ID);
    }
}
