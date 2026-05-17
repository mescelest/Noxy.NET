using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyIntegerCreate(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaPropertyIntegerCreate, ResponseSchemaPropertyIntegerCreate>
{
    public async ValueTask<ResponseSchemaPropertyIntegerCreate> Handle(CommandSchemaPropertyIntegerCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaPropertyInteger result = await uow.Schema.CreateSchemaPropertyInteger(new()
        {
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            SchemaIdentifier = request.SchemaIdentifier,
            Name = request.Name,
            Note = request.Note,
            Weight = request.Weight ?? BaseEntity.DefaultWeight,
            IsUnsigned = request.IsUnsigned,
            TitleTextParameterID = request.TitleParameterTextID,
            DescriptionTextParameterID = request.DescriptionParameterTextID,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
