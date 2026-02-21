using MediatR;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterList(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QueryDataParameterList, ResponseDataParameterList>
{
    public async Task<ResponseDataParameterList> Handle(QueryDataParameterList request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntityDataParameter.Discriminator> result = await uow.Data.GetParameterListWithIdentifier(request.SchemaIdentifier);

        return new() { Value = result };
    }
}
