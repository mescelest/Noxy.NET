using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterClone(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaParameterClone, ResponseSchemaParameterClone>
{
    public async ValueTask<ResponseSchemaParameterClone> Handle(CommandSchemaParameterClone command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaParameter entity = await uow.Schema.GetSchemaParameterByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(entity.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveParameterClone, ParameterSystemConstants.SchemaDeactivatedParameterClone);

        entity.ID = BaseEntity.CreateID();
        entity.SchemaIdentifier = BaseEntity.CreateTemporarySchemaIdentifier();
        entity.TimeCreated = DateTime.UtcNow;

        EntitySchemaParameter result = entity switch
        {
            EntitySchemaParameterStyle v => await uow.Schema.CreateSchemaParameterStyle(v),
            EntitySchemaParameterSystem v => await uow.Schema.CreateSchemaParameterSystem(v),
            EntitySchemaParameterText v => await uow.Schema.CreateSchemaParameterText(v),
            _ => throw new InvalidOperationException(),
        };

        await uow.Commit();

        return new(result.ID);
    }
}
