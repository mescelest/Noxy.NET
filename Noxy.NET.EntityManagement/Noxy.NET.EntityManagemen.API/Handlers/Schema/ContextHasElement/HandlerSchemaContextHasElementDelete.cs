using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.ContextHasElement;

public class HandlerSchemaContextHasElementDelete(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaContextHasElementDelete, ResponseSchemaContextHasElementDelete>
{
    public async ValueTask<ResponseSchemaContextHasElementDelete> Handle(CommandSchemaContextHasElementDelete command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaContextHasElement entity = await uow.Schema.GetSchemaContextHasElementByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(entity.Entity!.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveDeleteContextHasElement, ParameterSystemConstants.SchemaDeactivatedDeleteContextHasElement);

        uow.Schema.DeleteSchemaContextHasElement(entity);

        await uow.Commit();

        return new(entity.ID);
    }
}
