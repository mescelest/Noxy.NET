using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Element;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Element;

public class HandlerSchemaElementClone(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaElementClone, ResponseSchemaElementClone>
{
    public async ValueTask<ResponseSchemaElementClone> Handle(CommandSchemaElementClone command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaElement entity = await uow.Schema.GetSchemaElementByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(entity.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveCloneElement, ParameterSystemConstants.SchemaDeactivatedCloneElement);

        entity.ID = BaseEntity.CreateID();
        entity.SchemaIdentifier = BaseEntity.CreateTemporarySchemaIdentifier();
        entity.TimeCreated = DateTime.UtcNow;
        EntitySchemaElement result = await uow.Schema.CreateSchemaElement(entity);

        await uow.Commit();

        return new(result.ID);
    }
}
