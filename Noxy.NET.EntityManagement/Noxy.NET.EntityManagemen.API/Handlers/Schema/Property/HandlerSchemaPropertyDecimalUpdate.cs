using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyDecimalUpdate(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaPropertyDecimalUpdate, ResponseSchemaPropertyDecimalUpdate>
{
    public async ValueTask<ResponseSchemaPropertyDecimalUpdate> Handle(CommandSchemaPropertyDecimalUpdate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaPropertyDecimal result = await uow.Schema.UpdateSchemaPropertyDecimal(new()
        {
            ID = request.ID,
            SchemaID = Guid.Empty,
            SchemaIdentifier = request.SchemaIdentifier,
            Name = request.Name,
            Note = request.Note,
            Weight = request.Weight ?? BaseEntity.DefaultWeight,
            TitleTextParameterID = request.TitleParameterTextID,
            DescriptionTextParameterID = request.DescriptionParameterTextID,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
