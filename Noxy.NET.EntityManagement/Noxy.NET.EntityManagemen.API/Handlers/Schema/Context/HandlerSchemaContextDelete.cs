using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Context;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Context;

public class HandlerSchemaContextDelete(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaContextDelete, ResponseSchemaContextDelete>
{
    public async ValueTask<ResponseSchemaContextDelete> Handle(CommandSchemaContextDelete command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaContext entity = await uow.Schema.GetSchemaContextByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(entity.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveContextDelete, ParameterSystemConstants.SchemaDeactivatedContextDelete);

        uow.Schema.DeleteSchemaContext(entity);

        await uow.Commit();

        return new(entity.ID);
    }
}
