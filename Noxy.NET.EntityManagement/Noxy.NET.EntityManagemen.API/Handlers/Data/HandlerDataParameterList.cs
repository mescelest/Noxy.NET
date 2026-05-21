using System.ComponentModel;
using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Data;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterList(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QueryDataParameterList, ResponseDataParameterList>
{
    public async ValueTask<ResponseDataParameterList> Handle(QueryDataParameterList query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntityDataParameter> result = await uow.Data.GetParameterListByIdentifier(query.SchemaIdentifier, new()
        {
            Search = query.Search?.ToEscapedSqlLike(),
            PageSize = query.PageSize ?? 10,
            PageNumber = query.PageNumber ?? 0,
            SortColumn = query.SortColumn ?? nameof(EntityDataParameter.TimeEffective),
            SortDirection = query.SortDirection ?? ListSortDirection.Descending,
        });

        return new(result.Select(x => new EntityDataParameter.Discriminator(x)).ToList());
    }
}
