using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.ContextHasElement;

public class HandlerSchemaContextHasElementFind(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaContextHasElementFind, ResponseSchemaContextHasElementFind>
{
    public async ValueTask<ResponseSchemaContextHasElementFind> Handle(QuerySchemaContextHasElementFind request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaContextHasElement result = await uow.Schema.GetSchemaContextHasElementByID(request.ID);

        return new() { Value = result };
    }
}
