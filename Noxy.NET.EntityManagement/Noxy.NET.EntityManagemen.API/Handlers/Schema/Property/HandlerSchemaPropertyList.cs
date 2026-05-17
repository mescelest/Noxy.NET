using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyList(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaPropertyList, ResponseSchemaPropertyList>
{
    public async ValueTask<ResponseSchemaPropertyList> Handle(QuerySchemaPropertyList request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchemaProperty.Discriminator> result = await uow.Schema.GetSchemaPropertyList(new()
        {
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            Search = request.Search?.ToEscapedSqlLike(),
            PropertyType = request.PropertyType,
            PageSize = request.PageSize ?? 10,
            PageNumber = request.PageNumber ?? 0,
        });

        return new(result);
    }
}
