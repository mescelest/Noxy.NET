using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.Element;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Element;

public class HandlerSchemaElementCount(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaElementCount, ResponseSchemaElementCount>
{
    public async ValueTask<ResponseSchemaElementCount> Handle(QuerySchemaElementCount query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        int result = await uow.Schema.GetSchemaElementCount(new()
        {
            SchemaID = query.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            Search = query.Search?.ToEscapedSqlLike(),
        });

        return new(result);
    }
}
