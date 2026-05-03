using MediatR;
using Noxy.NET.EntityManagement.API.Queries.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaCount(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaCount, ResponseSchemaCount>
{
    public async Task<ResponseSchemaCount> Handle(QuerySchemaCount request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        int result = await uow.Schema.GetSchemaCount(new()
        {
            Search = request.Search?.ToEscapedSqlLike(),
        });

        return new() { Value = result };
    }
}
