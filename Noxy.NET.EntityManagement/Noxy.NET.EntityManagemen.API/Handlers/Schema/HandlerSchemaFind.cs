using MediatR;
using Noxy.NET.EntityManagement.API.Queries.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaFind(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaFind, ResponseSchemaFind>
{
    public async Task<ResponseSchemaFind> Handle(QuerySchemaFind request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema result = await uow.Schema.GetSchemaByID(request.ID);

        return new() { Value = result };
    }
}
