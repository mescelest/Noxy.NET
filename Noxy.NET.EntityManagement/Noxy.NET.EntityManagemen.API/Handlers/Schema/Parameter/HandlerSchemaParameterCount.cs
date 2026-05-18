using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterCount(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaParameterCount, ResponseSchemaParameterCount>
{
    public async ValueTask<ResponseSchemaParameterCount> Handle(QuerySchemaParameterCount query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        int result = await uow.Schema.GetSchemaParameterCount(new()
        {
            SchemaID = query.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            Search = query.Search?.ToEscapedSqlLike(),
            IsSystemDefined = query.IsSystemDefined,
            IsApprovalRequired = query.IsApprovalRequired,
            ParameterType = query.ParameterType,
        });

        return new(result);
    }
}
