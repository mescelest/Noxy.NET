using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterList(IUnitOfWorkFactory serviceUoWFactory) : IQueryHandler<QuerySchemaParameterList, ResponseSchemaParameterList>
{
    public async ValueTask<ResponseSchemaParameterList> Handle(QuerySchemaParameterList query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchemaParameter.Discriminator> result = await uow.Schema.GetSchemaParameterList(new()
        {
            SchemaID = query.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            Search = query.Search?.ToEscapedSqlLike(),
            IsSystemDefined = query.IsSystemDefined,
            IsApprovalRequired = query.IsApprovalRequired,
            ParameterType = query.ParameterType,
            PageSize = query.PageSize ?? 10,
            PageNumber = query.PageNumber ?? 0,
        });

        return new(result);
    }
}
