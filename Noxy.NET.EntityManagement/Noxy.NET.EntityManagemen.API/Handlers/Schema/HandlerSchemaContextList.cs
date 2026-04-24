using MediatR;
using Noxy.NET.EntityManagement.API.Queries.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaContextList(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaContextList, ResponseSchemaContextList>
{
    public async Task<ResponseSchemaContextList> Handle(QuerySchemaContextList request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchemaContext> result = await uow.Schema.GetSchemaContextList(new()
        {
            Search = request.Search,
            PageSize = request.PageSize ?? 10,
            PageNumber = request.PageNumber ?? 0,
        });

        return new() { Value = result };
    }
}
