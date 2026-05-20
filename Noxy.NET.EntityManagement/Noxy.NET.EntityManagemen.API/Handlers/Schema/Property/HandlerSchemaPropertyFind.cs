using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyFind(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaPropertyFind, ResponseSchemaPropertyFind>
{
    public async ValueTask<ResponseSchemaPropertyFind> Handle(QuerySchemaPropertyFind query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaProperty result = await uow.Schema.GetSchemaPropertyByID(query.ID);

        return new(new(result));
    }
}
