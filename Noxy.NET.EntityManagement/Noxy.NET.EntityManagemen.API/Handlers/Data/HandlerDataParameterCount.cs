using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Data;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterCount(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QueryDataParameterCount, ResponseDataParameterCount>
{
    public async ValueTask<ResponseDataParameterCount> Handle(QueryDataParameterCount request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        int result = await uow.Data.GetParameterCountByIdentifier(request.SchemaIdentifier, new()
        {
            Search = request.Search
        });

        return new(result);
    }
}
