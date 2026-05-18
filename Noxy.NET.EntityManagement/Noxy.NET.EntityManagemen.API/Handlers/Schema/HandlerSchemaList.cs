using System.ComponentModel;
using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaList(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaList, ResponseSchemaList>
{
    public async ValueTask<ResponseSchemaList> Handle(QuerySchemaList query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchema> result = await uow.Schema.GetSchemaList(new()
        {
            IsActivated = query.IsActivated,
            Search = query.Search?.ToEscapedSqlLike(),
            PageSize = query.PageSize ?? 10,
            PageNumber = query.PageNumber ?? 0,
            SortColumn = query.SortColumn ?? nameof(EntitySchema.TimeCreated),
            SortDirection = query.SortDirection ?? ListSortDirection.Descending,
        });

        return new(result);
    }
}
