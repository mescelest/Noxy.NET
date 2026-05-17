using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterTextUpdate(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandSchemaParameterTextUpdate, ResponseSchemaParameterTextUpdate>
{
    public async ValueTask<ResponseSchemaParameterTextUpdate> Handle(CommandSchemaParameterTextUpdate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaParameterText result = await uow.Schema.UpdateSchemaParameterText(new()
        {
            ID = request.ID,
            SchemaID = Guid.Empty,
            SchemaIdentifier = request.SchemaIdentifier,
            Name = request.Name,
            Note = request.Note,
            IsApprovalRequired = request.IsApprovalRequired,
            IsSystemDefined = request.IsSystemDefined,
            Type = request.Type,
        });

        await uow.Commit();

        return new() { Value = result };
    }
}
