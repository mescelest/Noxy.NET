using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyDateTimeCreate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaPropertyDateTimeCreate, ResponseSchemaPropertyDateTimeCreate>
{
    public async ValueTask<ResponseSchemaPropertyDateTimeCreate> Handle(CommandSchemaPropertyDateTimeCreate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema schema = await uow.Schema.GetSchemaByID(command.SchemaID ?? await uow.Schema.GetCurrentSchemaID());
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactiveAddProperty, ParameterSystemConstants.SchemaDeactivatedAddProperty);

        EntitySchemaPropertyDateTime result = await uow.Schema.CreateSchemaPropertyDateTime(new()
        {
            SchemaID = command.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            SchemaIdentifier = command.SchemaIdentifier,
            Name = command.Name,
            Note = command.Note,
            Type = command.Type,
            Weight = command.Weight ?? BaseEntity.DefaultWeight,
            TitleParameterTextID = command.TitleParameterTextID,
            DescriptionParameterTextID = command.DescriptionParameterTextID,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
