using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Context;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Context;

public class HandlerSchemaContextUpdate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaContextUpdate, ResponseSchemaContextUpdate>
{
    public async ValueTask<ResponseSchemaContextUpdate> Handle(CommandSchemaContextUpdate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaContext entity = await uow.Schema.GetSchemaContextByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(entity.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveEditContext, ParameterSystemConstants.SchemaDeactivatedEditContext);

        entity.SchemaIdentifier = command.SchemaIdentifier;
        entity.Name = command.Name;
        entity.Note = command.Note;
        entity.TitleTextParameterID = command.TitleParameterTextID;
        entity.DescriptionTextParameterID = command.DescriptionParameterTextID;
        uow.Schema.UpdateSchemaContext(entity);

        await uow.Commit();

        return new(entity.ID);
    }
}
