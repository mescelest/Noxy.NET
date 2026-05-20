using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterApprove(IUnitOfWorkFactory serviceUoWFactory) : ICommandHandler<CommandDataParameterApprove, ResponseDataParameterApprove>
{
    public async ValueTask<ResponseDataParameterApprove> Handle(CommandDataParameterApprove request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityDataParameter.Discriminator discriminator = await uow.Data.GetParameterByID(request.ID);
        EntityDataParameter entity = discriminator.GetValue();
        if (entity.TimeApproved.HasValue) return new(entity.TimeApproved.Value);

        entity.TimeApproved = DateTime.UtcNow;
        uow.Data.UpdateParameter(entity);

        await uow.Commit();

        return new(entity.TimeApproved.Value);
    }
}
