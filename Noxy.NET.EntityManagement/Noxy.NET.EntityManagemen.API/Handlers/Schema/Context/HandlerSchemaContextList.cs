using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.Context;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Context;

public class HandlerSchemaContextList(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaContextList, ResponseSchemaContextList>
{
    public async ValueTask<ResponseSchemaContextList> Handle(QuerySchemaContextList query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchemaContext> result = await uow.Schema.GetSchemaContextList(new()
        {
            SchemaID = query.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            Search = query.Search?.ToEscapedSqlLike(),
            PageSize = query.PageSize ?? 10,
            PageNumber = query.PageNumber ?? 0,
        });

        return new(result);
    }
}
