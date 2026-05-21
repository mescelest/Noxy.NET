using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaUpdate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaUpdate, ResponseSchemaUpdate>
{
    public async ValueTask<ResponseSchemaUpdate> Handle(CommandSchemaUpdate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema result = await uow.Schema.GetSchemaByID(command.ID);
        serviceSchemaValidator.ValidateSchemaChange(result, ParameterSystemConstants.SchemaInactiveEntityEdit, ParameterSystemConstants.SchemaDeactivatedEntityEdit);

        result.Name = command.Name;
        result.Note = command.Note;
        result.TimeUpdated = DateTime.UtcNow;

        uow.Schema.UpdateSchema(result);
        await uow.Commit();

        return new(result.ID);
    }
}
