using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.ElementHasProperty;

public class HandlerSchemaElementHasPropertyFind(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaElementHasPropertyFind, ResponseSchemaElementHasPropertyFind>
{
    public async ValueTask<ResponseSchemaElementHasPropertyFind> Handle(QuerySchemaElementHasPropertyFind query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaElementHasProperty result = await uow.Schema.GetSchemaElementHasPropertyByID(query.ID);

        return new(result);
    }
}
