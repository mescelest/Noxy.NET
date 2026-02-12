using MediatR;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Handlers;

public class HandlerDataParameterStyleCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QueryDataParameterStyleCreate, ResponseDataParameterStyleCreate>
{
    public async Task<ResponseDataParameterStyleCreate> Handle(QueryDataParameterStyleCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityDataParameterStyle result = await uow.Data.CreateStyleParameter(request.SchemaIdentifier, request.Value, request.DateEffective);

        await uow.Commit();

        return new() { Value = result };
    }
}
