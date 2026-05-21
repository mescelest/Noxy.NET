using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterStyleUpdate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaParameterStyleUpdate, ResponseSchemaParameterStyleUpdate>
{
    public async ValueTask<ResponseSchemaParameterStyleUpdate> Handle(CommandSchemaParameterStyleUpdate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaParameter discriminator = await uow.Schema.GetSchemaParameterByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(discriminator.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveParameterEdit, ParameterSystemConstants.SchemaDeactivatedParameterEdit);
        if (discriminator is not EntitySchemaParameterStyle entity) throw new InvalidOperationException("Parameter is not of type Style");

        entity.SchemaIdentifier = command.SchemaIdentifier;
        entity.Name = command.Name;
        entity.Note = command.Note;
        entity.IsApprovalRequired = command.IsApprovalRequired;
        entity.IsSystemDefined = command.IsSystemDefined;
        uow.Schema.UpdateSchemaParameterStyle(entity);

        await uow.Commit();

        return new(entity.ID);
    }
}
