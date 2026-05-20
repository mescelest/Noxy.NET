using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Data;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterStyleCreate(IUnitOfWorkFactory serviceUoWFactory, IParameterService serviceParameter) : ICommandHandler<CommandDataParameterStyleCreate, ResponseDataParameterStyleCreate>
{
    public async ValueTask<ResponseDataParameterStyleCreate> Handle(CommandDataParameterStyleCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid id = await uow.Schema.GetCurrentSchemaID();
        EntitySchemaParameter parameter = await uow.Schema.GetSchemaParameterByIdentifier(id, request.SchemaIdentifier);
        if (parameter is not EntitySchemaParameterStyle schema) throw new InvalidOperationException("SchemaParameter is not of type Style");

        EntityDataParameterStyle entity = new()
        {
            SchemaIdentifier = request.SchemaIdentifier,
            Value = request.Value,
            TimeApproved = schema.IsApprovalRequired ? null : DateTime.UtcNow,
            TimeEffective = request.DateEffective ?? DateTime.UtcNow,
        };
        EntityDataParameterStyle result = await uow.Data.CreateParameterStyle(entity);

        await uow.Commit();

        serviceParameter.AddToCache(result);

        return new(result);
    }
}
