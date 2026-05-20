using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Data;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterSystemCreate(IUnitOfWorkFactory serviceUoWFactory, IParameterService serviceParameter) : ICommandHandler<CommandDataParameterSystemCreate, ResponseDataParameterSystemCreate>
{
    public async ValueTask<ResponseDataParameterSystemCreate> Handle(CommandDataParameterSystemCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid id = await uow.Schema.GetCurrentSchemaID();
        EntitySchemaParameter.Discriminator discriminator = await uow.Schema.GetSchemaParameterByIdentifier(id, request.SchemaIdentifier);
        if (discriminator.GetValue() is not EntitySchemaParameterSystem schema) throw new InvalidOperationException("SchemaParameter is not of type System");

        EntityDataParameterSystem entity = new()
        {
            SchemaIdentifier = request.SchemaIdentifier,
            Value = request.Value,
            TimeApproved = schema.IsApprovalRequired ? null : DateTime.UtcNow,
            TimeEffective = request.DateEffective ?? DateTime.UtcNow,
        };
        EntityDataParameterSystem result = await uow.Data.CreateParameterSystem(entity);

        await uow.Commit();

        serviceParameter.SetParameter(discriminator, new(result));

        return new(result);
    }
}
