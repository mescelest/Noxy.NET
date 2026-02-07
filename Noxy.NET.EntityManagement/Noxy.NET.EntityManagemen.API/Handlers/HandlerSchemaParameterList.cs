using MediatR;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Handlers;

public class HandlerSchemaParameterList(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaParameterList, ResponseSchemaParameterList>
{
    public async Task<ResponseSchemaParameterList> Handle(QuerySchemaParameterList request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchemaParameter.Discriminator> result = await uow.Schema.GetSchemaParameterList(request.Search, request.IsSystemDefined, request.IsApprovalRequired);

        return new() { Value = result };
    }
}
