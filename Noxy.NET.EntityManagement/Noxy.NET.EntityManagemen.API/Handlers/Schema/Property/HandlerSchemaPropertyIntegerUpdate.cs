using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyIntegerUpdate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaPropertyIntegerUpdate, ResponseSchemaPropertyIntegerUpdate>
{
    public async Task<ResponseSchemaPropertyIntegerUpdate> Handle(CommandSchemaPropertyIntegerUpdate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaPropertyInteger result = await uow.Schema.UpdateSchemaPropertyInteger(new()
        {
            ID = request.ID,
            SchemaID = Guid.Empty,
            SchemaIdentifier = request.SchemaIdentifier,
            Name = request.Name,
            Note = request.Note,
            Weight = request.Weight ?? BaseEntity.DefaultWeight,
            IsUnsigned = request.IsUnsigned,
            TitleTextParameterID = request.TitleParameterTextID,
            DescriptionTextParameterID = request.DescriptionParameterTextID,
        });

        await uow.Commit();

        return new() { Value = result };
    }
}
