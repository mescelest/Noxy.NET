using MediatR;
using Noxy.NET.EntityManagement.API.Queries.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterFind(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaParameterFind, ResponseSchemaParameterFind>
{
    public async Task<ResponseSchemaParameterFind> Handle(QuerySchemaParameterFind request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaParameter.Discriminator result = await uow.Schema.GetSchemaParameterByID(request.ID);

        return new() { Value = result };
    }
}
