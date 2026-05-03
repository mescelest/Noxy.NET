using MediatR;
using Noxy.NET.EntityManagement.API.Queries.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterCount(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaParameterCount, ResponseSchemaParameterCount>
{
    public async Task<ResponseSchemaParameterCount> Handle(QuerySchemaParameterCount request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        int result = await uow.Schema.GetSchemaParameterCount(new()
        {
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            Search = request.Search?.ToEscapedSqlLike(),
            IsSystemDefined = request.IsSystemDefined,
            IsApprovalRequired = request.IsApprovalRequired,
            ParameterType = request.ParameterType,
        });

        return new() { Value = result };
    }
}
