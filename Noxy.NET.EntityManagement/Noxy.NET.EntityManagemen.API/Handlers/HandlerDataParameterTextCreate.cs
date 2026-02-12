using MediatR;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Handlers;

public class HandlerDataParameterTextCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QueryDataParameterTextCreate, ResponseDataParameterTextCreate>
{
    public async Task<ResponseDataParameterTextCreate> Handle(QueryDataParameterTextCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityDataParameterText result = await uow.Data.CreateTextParameter(request.SchemaIdentifier, request.Value, request.DateEffective);

        await uow.Commit();

        return new() { Value = result };
    }
}
