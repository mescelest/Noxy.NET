using MediatR;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaParameterList(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaParameterList, ResponseSchemaParameterList>
{
    public async Task<ResponseSchemaParameterList> Handle(QuerySchemaParameterList request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchemaParameter.Discriminator> result = await uow.Schema.GetSchemaParameterList(new()
        {
            Search = request.Search,
            IsSystemDefined = request.IsSystemDefined,
            IsApprovalRequired = request.IsApprovalRequired,
            ParameterType = request.ParameterType,
            PageSize = request.PageSize ?? 10,
            PageNumber = request.PageNumber ?? 0,
        });

        return new() { Value = result };
    }
}
