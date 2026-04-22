using MediatR;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaList(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaList, ResponseSchemaList>
{
    public async Task<ResponseSchemaList> Handle(QuerySchemaList request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchema> result = await uow.Schema.GetSchemaList(new()
        {
            Search = request.Search,
            PageSize = request.PageSize ?? 10,
            PageNumber = request.PageNumber ?? 0,
        });

        return new() { Value = result };
    }
}
