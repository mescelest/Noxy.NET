using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaCount(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaCount, ResponseSchemaCount>
{
    public async ValueTask<ResponseSchemaCount> Handle(QuerySchemaCount query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        int result = await uow.Schema.GetSchemaCount(new()
        {
            Search = query.Search?.ToEscapedSqlLike(),
        });

        return new(result);
    }
}
