using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Data;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterTextCreate(IUnitOfWorkFactory serviceUoWFactory, IParameterService serviceParameter) : ICommandHandler<CommandDataParameterTextCreate, ResponseDataParameterTextCreate>
{
    public async ValueTask<ResponseDataParameterTextCreate> Handle(CommandDataParameterTextCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid id = await uow.Schema.GetCurrentSchemaID();
        EntitySchemaParameter discriminator = await uow.Schema.GetSchemaParameterByIdentifier(id, request.SchemaIdentifier);
        if (discriminator is not EntitySchemaParameterText schema) throw new InvalidOperationException("SchemaParameter is not of type Text");

        EntityDataParameterText entity = new()
        {
            SchemaIdentifier = request.SchemaIdentifier,
            Value = request.Value,
            TimeApproved = schema.IsApprovalRequired ? null : DateTime.UtcNow,
            TimeEffective = request.DateEffective ?? DateTime.UtcNow,
        };
        EntityDataParameterText result = await uow.Data.CreateParameterText(entity);

        await uow.Commit();

        serviceParameter.AddToCache(result);

        return new(result.ID);
    }
}
