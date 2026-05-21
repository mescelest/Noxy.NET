using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Element;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Element;

public class HandlerSchemaElementCreate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaElementCreate, ResponseSchemaElementCreate>
{
    public async ValueTask<ResponseSchemaElementCreate> Handle(CommandSchemaElementCreate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema schema = await uow.Schema.GetSchemaByID(command.SchemaID ?? await uow.Schema.GetCurrentSchemaID());
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveElementAdd, ParameterSystemConstants.SchemaDeactivatedElementAdd);

        EntitySchemaElement result = await uow.Schema.CreateSchemaElement(new()
        {
            SchemaID = schema.ID,
            SchemaIdentifier = command.SchemaIdentifier,
            Name = command.Name,
            Note = command.Note,
            TitleParameterTextID = command.TitleParameterTextID,
            DescriptionParameterTextID = command.DescriptionParameterTextID,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
