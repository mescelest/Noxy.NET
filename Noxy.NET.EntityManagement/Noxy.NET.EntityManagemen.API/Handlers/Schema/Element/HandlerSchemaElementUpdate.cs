using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Element;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Element;

public class HandlerSchemaElementUpdate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaElementUpdate, ResponseSchemaElementUpdate>
{
    public async ValueTask<ResponseSchemaElementUpdate> Handle(CommandSchemaElementUpdate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaElement entity = await uow.Schema.GetSchemaElementByID(command.ID);
        EntitySchema schema = await uow.Schema.GetSchemaByID(entity.SchemaID);
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveElementEdit, ParameterSystemConstants.SchemaDeactivatedElementEdit);

        entity.SchemaIdentifier = command.SchemaIdentifier;
        entity.Name = command.Name;
        entity.Note = command.Note;
        entity.TitleParameterTextID = command.TitleParameterTextID;
        entity.DescriptionParameterTextID = command.DescriptionParameterTextID;
        uow.Schema.UpdateSchemaElement(entity);

        await uow.Commit();

        return new(entity.ID);
    }
}
