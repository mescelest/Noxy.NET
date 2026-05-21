using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyImageCreate(IUnitOfWorkFactory serviceUoWFactory, ISchemaValidatorService serviceSchemaValidator) : ICommandHandler<CommandSchemaPropertyImageCreate, ResponseSchemaPropertyImageCreate>
{
    public async ValueTask<ResponseSchemaPropertyImageCreate> Handle(CommandSchemaPropertyImageCreate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema schema = await uow.Schema.GetSchemaByID(command.SchemaID ?? await uow.Schema.GetCurrentSchemaID());
        serviceSchemaValidator.ValidateSchemaChange(schema, ParameterSystemConstants.SchemaInactivePropertyAdd, ParameterSystemConstants.SchemaDeactivatedPropertyAdd);

        EntitySchemaPropertyImage result = await uow.Schema.CreateSchemaPropertyImage(new()
        {
            SchemaID = command.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            SchemaIdentifier = command.SchemaIdentifier,
            Name = command.Name,
            Note = command.Note,
            Weight = command.Weight ?? BaseEntity.DefaultWeight,
            AllowedExtensions = command.AllowedExtensions,
            TitleParameterTextID = command.TitleParameterTextID,
            DescriptionParameterTextID = command.DescriptionParameterTextID,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
