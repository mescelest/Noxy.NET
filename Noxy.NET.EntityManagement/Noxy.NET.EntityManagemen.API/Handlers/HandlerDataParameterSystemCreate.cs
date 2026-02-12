using MediatR;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Handlers;

public class HandlerDataParameterSystemCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QueryDataParameterSystemCreate, ResponseDataParameterSystemCreate>
{
    public async Task<ResponseDataParameterSystemCreate> Handle(QueryDataParameterSystemCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityDataParameterSystem result = await uow.Data.CreateSystemParameter(request.SchemaIdentifier, request.Value, request.DateEffective);

        await uow.Commit();

        return new() { Value = result };
    }
}
