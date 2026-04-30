using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterStyleUpdate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaParameterStyleUpdate, ResponseSchemaParameterStyleUpdate>
{
    public async Task<ResponseSchemaParameterStyleUpdate> Handle(CommandSchemaParameterStyleUpdate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaParameterStyle result = await uow.Schema.UpdateSchemaParameterStyle(new()
        {
            ID = request.ID,
            SchemaID = Guid.Empty,
            SchemaIdentifier = request.SchemaIdentifier,
            Name = request.Name,
            Note = request.Note,
            IsApprovalRequired = request.IsApprovalRequired,
            IsSystemDefined = request.IsSystemDefined,
        });

        await uow.Commit();

        return new() { Value = result };
    }
}
