using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyDecimalCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaPropertyDecimalCreate, ResponseSchemaPropertyDecimalCreate>
{
    public async Task<ResponseSchemaPropertyDecimalCreate> Handle(CommandSchemaPropertyDecimalCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaPropertyDecimal result = await uow.Schema.CreateSchemaPropertyDecimal(new()
        {
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            SchemaIdentifier = request.SchemaIdentifier,
            Name = request.Name,
            Note = request.Note,
        });

        await uow.Commit();

        return new() { Value = result };
    }
}
