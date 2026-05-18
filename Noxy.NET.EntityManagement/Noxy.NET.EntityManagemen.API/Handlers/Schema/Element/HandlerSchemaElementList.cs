using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.Element;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Element;

public class HandlerSchemaElementList(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaElementList, ResponseSchemaElementList>
{
    public async ValueTask<ResponseSchemaElementList> Handle(QuerySchemaElementList query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchemaElement> result = await uow.Schema.GetSchemaElementList(new()
        {
            SchemaID = query.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            Search = query.Search?.ToEscapedSqlLike(),
            PageSize = query.PageSize ?? 10,
            PageNumber = query.PageNumber ?? 0,
        });

        return new(result);
    }
}
