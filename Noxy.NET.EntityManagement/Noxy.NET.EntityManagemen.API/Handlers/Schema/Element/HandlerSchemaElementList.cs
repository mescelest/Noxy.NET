using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.Element;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Element;

public class HandlerSchemaElementList(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaElementList, ResponseSchemaElementList>
{
    public async ValueTask<ResponseSchemaElementList> Handle(QuerySchemaElementList request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchemaElement> result = await uow.Schema.GetSchemaElementList(new()
        {
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            Search = request.Search?.ToEscapedSqlLike(),
            PageSize = request.PageSize ?? 10,
            PageNumber = request.PageNumber ?? 0,
        });

        return new(result);
    }
}
