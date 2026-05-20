using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Data;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterDelete(IUnitOfWorkFactory serviceUoWFactory, IParameterService serviceParameter) : ICommandHandler<CommandDataParameterDelete, ResponseDataParameterDelete>
{
    public async ValueTask<ResponseDataParameterDelete> Handle(CommandDataParameterDelete request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityDataParameter entity = await uow.Data.GetParameterByID(request.ID);
        if (entity.TimeEffective <= DateTime.UtcNow) throw new InvalidOperationException("Cannot delete parameter that is already effective.");

        uow.Data.RemoveParameter(entity);
        await uow.Commit();

        serviceParameter.AddToCache(entity);

        return new() { ID = entity.ID };
    }
}
