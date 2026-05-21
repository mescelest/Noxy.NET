using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Data;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterApprove(IUnitOfWorkFactory serviceUoWFactory, IParameterService serviceParameter) : ICommandHandler<CommandDataParameterApprove, ResponseDataParameterApprove>
{
    public async ValueTask<ResponseDataParameterApprove> Handle(CommandDataParameterApprove request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityDataParameter entity = await uow.Data.GetParameterByID(request.ID);
        if (entity.TimeApproved.HasValue) return new(entity.TimeApproved.Value);

        entity.TimeApproved = DateTime.UtcNow;
        uow.Data.UpdateParameter(entity);
        await uow.Commit();

        serviceParameter.ReplaceInCache(entity);

        return new(entity.TimeApproved.Value);
    }
}
