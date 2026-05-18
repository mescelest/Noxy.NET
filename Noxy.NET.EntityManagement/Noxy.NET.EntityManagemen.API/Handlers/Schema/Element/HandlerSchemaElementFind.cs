using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.Element;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Element;

public class HandlerSchemaElementFind(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaElementFind, ResponseSchemaElementFind>
{
    public async ValueTask<ResponseSchemaElementFind> Handle(QuerySchemaElementFind query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaElement result = await uow.Schema.GetSchemaElementByID(query.ID);

        return new(result);
    }
}
