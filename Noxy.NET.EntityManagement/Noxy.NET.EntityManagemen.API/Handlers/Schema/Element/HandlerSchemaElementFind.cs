using MediatR;
using Noxy.NET.EntityManagement.API.Queries.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Element;

public class HandlerSchemaElementFind(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaElementFind, ResponseSchemaElementFind>
{
    public async Task<ResponseSchemaElementFind> Handle(QuerySchemaElementFind request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaElement result = await uow.Schema.GetSchemaElementByID(request.ID);

        return new() { Value = result };
    }
}
