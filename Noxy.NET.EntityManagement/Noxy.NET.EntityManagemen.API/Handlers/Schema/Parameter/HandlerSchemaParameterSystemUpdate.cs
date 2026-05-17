using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterSystemUpdate(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaParameterSystemUpdate, ResponseSchemaParameterSystemUpdate>
{
    public async ValueTask<ResponseSchemaParameterSystemUpdate> Handle(CommandSchemaParameterSystemUpdate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaParameterSystem result = await uow.Schema.UpdateSchemaParameterSystem(new()
        {
            ID = request.ID,
            SchemaID = Guid.Empty,
            SchemaIdentifier = request.SchemaIdentifier,
            Name = request.Name,
            Note = request.Note,
            Type = request.Type,
            IsApprovalRequired = request.IsApprovalRequired,
            IsSystemDefined = request.IsSystemDefined,
        });

        await uow.Commit();

        return new() { Value = result };
    }
}
