using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterTextCreate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaParameterTextCreate, ResponseSchemaParameterTextCreate>
{
    public async ValueTask<ResponseSchemaParameterTextCreate> Handle(CommandSchemaParameterTextCreate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema schema = await uow.Schema.GetSchemaByID(command.SchemaID ?? await uow.Schema.GetCurrentSchemaID());
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveAddElement, ParameterSystemConstants.SchemaDeactivatedAddElement);

        EntitySchemaParameterText result = await uow.Schema.CreateSchemaParameterText(new()
        {
            SchemaID = command.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            SchemaIdentifier = command.SchemaIdentifier,
            Name = command.Name,
            Note = command.Note,
            Type = command.Type,
            IsApprovalRequired = command.IsApprovalRequired,
            IsSystemDefined = command.IsSystemDefined,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
