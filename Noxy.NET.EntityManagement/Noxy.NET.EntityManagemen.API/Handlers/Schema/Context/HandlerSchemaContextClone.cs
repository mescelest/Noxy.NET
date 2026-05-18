using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Context;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Context;

public class HandlerSchemaContextClone(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaContextClone, ResponseSchemaContextClone>
{
    public async ValueTask<ResponseSchemaContextClone> Handle(CommandSchemaContextClone command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaContext entity = await uow.Schema.GetSchemaContextByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(entity.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveCloneContext, ParameterSystemConstants.SchemaDeactivatedCloneContext);

        entity.ID = BaseEntity.CreateID();
        entity.SchemaIdentifier = BaseEntity.CreateTemporarySchemaIdentifier();
        entity.TimeCreated = DateTime.UtcNow;
        EntitySchemaContext result = await uow.Schema.CreateSchemaContext(entity);

        await uow.Commit();

        return new(result.ID);
    }
}
