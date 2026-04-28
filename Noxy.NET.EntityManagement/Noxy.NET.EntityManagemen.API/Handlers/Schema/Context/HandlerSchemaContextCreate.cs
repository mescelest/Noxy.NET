using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema.Context;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Context;

public class HandlerSchemaContextCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaContextCreate, ResponseSchemaContextCreate>
{
    public async Task<ResponseSchemaContextCreate> Handle(CommandSchemaContextCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaContext result = await uow.Schema.Create(new EntitySchemaContext
        {
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            SchemaIdentifier = request.SchemaIdentifier,
            Name = request.Name,
            Note = request.Note ?? string.Empty,
            TitleTextParameterID = request.TitleParameterTextID,
            DescriptionTextParameterID = request.DescriptionParameterTextID,
        });

        await uow.Commit();

        return new() { Value = result };
    }
}
