using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Context;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Context;

public class HandlerSchemaContextCreate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaContextCreate, ResponseSchemaContextCreate>
{
    public async ValueTask<ResponseSchemaContextCreate> Handle(CommandSchemaContextCreate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema schema = await uow.Schema.GetSchemaByID(command.SchemaID ?? await uow.Schema.GetCurrentSchemaID());
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveContextAdd, ParameterSystemConstants.SchemaDeactivatedContextAdd);

        EntitySchemaContext result = await uow.Schema.CreateSchemaContext(new()
        {
            SchemaID = schema.ID,
            SchemaIdentifier = command.SchemaIdentifier,
            Name = command.Name,
            Note = command.Note ?? string.Empty,
            TitleTextParameterID = command.TitleParameterTextID,
            DescriptionTextParameterID = command.DescriptionParameterTextID,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
