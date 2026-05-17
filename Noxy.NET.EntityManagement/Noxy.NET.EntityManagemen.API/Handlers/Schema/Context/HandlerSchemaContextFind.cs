using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.Context;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Context;

public class HandlerSchemaContextFind(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaContextFind, ResponseSchemaContextFind>
{
    public async ValueTask<ResponseSchemaContextFind> Handle(QuerySchemaContextFind request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaContext result = await uow.Schema.GetSchemaContextByID(request.ID);

        return new(result);
    }
}
