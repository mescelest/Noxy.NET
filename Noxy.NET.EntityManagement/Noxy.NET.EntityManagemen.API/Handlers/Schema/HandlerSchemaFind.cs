using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaFind(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaFind, ResponseSchemaFind>
{
    public async ValueTask<ResponseSchemaFind> Handle(QuerySchemaFind query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Guid id = query.ID != Guid.Empty ? query.ID : await uow.Schema.GetCurrentSchemaID();
        EntitySchema result = await uow.Schema.GetSchemaByID(id);

        return new(result);
    }
}
