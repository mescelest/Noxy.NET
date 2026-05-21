using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Data;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterFind(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QueryDataParameterFind, ResponseDataParameterFind>
{
    public async ValueTask<ResponseDataParameterFind> Handle(QueryDataParameterFind request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityDataParameter result = await uow.Data.GetParameterByID(request.ID);

        return new(new(result));
    }
}
