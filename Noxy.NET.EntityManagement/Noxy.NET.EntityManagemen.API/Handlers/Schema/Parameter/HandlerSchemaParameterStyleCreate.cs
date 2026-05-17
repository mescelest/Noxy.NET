using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterStyleCreate(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaParameterStyleCreate, ResponseSchemaParameterStyleCreate>
{
    public async ValueTask<ResponseSchemaParameterStyleCreate> Handle(CommandSchemaParameterStyleCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaParameterStyle result = await uow.Schema.CreateSchemaParameterStyle(new()
        {
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            SchemaIdentifier = request.SchemaIdentifier,
            Name = request.Name,
            Note = request.Note,
            IsApprovalRequired = request.IsApprovalRequired,
            IsSystemDefined = request.IsSystemDefined,
        });

        await uow.Commit();

        return new(result.ID);
    }
}
