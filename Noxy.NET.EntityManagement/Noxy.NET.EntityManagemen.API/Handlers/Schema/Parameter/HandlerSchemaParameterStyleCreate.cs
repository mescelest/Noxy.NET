using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterStyleCreate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaParameterStyleCreate, ResponseSchemaParameterStyleCreate>
{
    public async ValueTask<ResponseSchemaParameterStyleCreate> Handle(CommandSchemaParameterStyleCreate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema schema = await uow.Schema.GetSchemaByID(command.SchemaID ?? await uow.Schema.GetCurrentSchemaID());
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveParameterAdd, ParameterSystemConstants.SchemaDeactivatedParameterAdd);

        EntitySchemaParameterStyle result = await uow.Schema.CreateSchemaParameterStyle(new()
        {
            SchemaID = command.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            SchemaIdentifier = command.SchemaIdentifier,
            Name = command.Name,
            Note = command.Note,
            IsApprovalRequired = command.IsApprovalRequired,
            IsSystemDefined = command.IsSystemDefined,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
