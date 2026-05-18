using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyCount(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaPropertyCount, ResponseSchemaPropertyCount>
{
    public async ValueTask<ResponseSchemaPropertyCount> Handle(QuerySchemaPropertyCount query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        int result = await uow.Schema.GetSchemaPropertyCount(new()
        {
            SchemaID = query.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            Search = query.Search?.ToEscapedSqlLike(),
            PropertyType = query.PropertyType,
        });

        return new(result);
    }
}
