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

        Guid id = request.ID != Guid.Empty ? request.ID : await uow.Schema.GetCurrentSchemaID();
        EntitySchema result = await uow.Schema.GetSchemaByID(id);

        return new() { Value = result };
    }
}
