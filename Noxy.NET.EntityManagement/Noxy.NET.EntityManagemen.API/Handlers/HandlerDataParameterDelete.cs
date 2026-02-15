using MediatR;
using Noxy.NET.EntityManagement.API.Commands;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Handlers;

public class HandlerDataParameterDelete(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandDataParameterDelete, ResponseDataParameterDelete>
{
    public async Task<ResponseDataParameterDelete> Handle(CommandDataParameterDelete request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid result = await uow.Data.RemoveParameterByID(request.ID);

        await uow.Commit();

        return new() { ID = result };
    }
}
