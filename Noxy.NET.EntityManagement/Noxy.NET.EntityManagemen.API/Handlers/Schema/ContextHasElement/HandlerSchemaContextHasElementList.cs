using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.ContextHasElement;

public class HandlerSchemaContextHasElementList(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaContextHasElementList, ResponseSchemaContextHasElementList>
{
    public async ValueTask<ResponseSchemaContextHasElementList> Handle(QuerySchemaContextHasElementList request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchemaContextHasElement> result = await uow.Schema.GetSchemaContextHasElementList(new()
        {
            SchemaContextID = request.SchemaContextID,
            SchemaElementID = request.SchemaElementID,
        });

        return new() { Value = result };
    }
}
