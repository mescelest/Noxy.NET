using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.Context;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Context;

public class HandlerSchemaContextCount(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaContextCount, ResponseSchemaContextCount>
{
    public async ValueTask<ResponseSchemaContextCount> Handle(QuerySchemaContextCount request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        int result = await uow.Schema.GetSchemaContextCount(new()
        {
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            Search = request.Search?.ToEscapedSqlLike(),
        });

        return new() { Value = result };
    }
}
