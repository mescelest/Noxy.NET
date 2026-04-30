using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterSystemCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaParameterSystemCreate, ResponseSchemaParameterSystemCreate>
{
    public async Task<ResponseSchemaParameterSystemCreate> Handle(CommandSchemaParameterSystemCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaParameterSystem result = await uow.Schema.CreateSchemaParameterSystem(new()
        {
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
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
