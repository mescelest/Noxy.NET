using MediatR;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Handlers;

public class HandlerDataParameterTextResolve(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QueryDataParameterTextResolve, ResponseDataParameterResolve>
{
    public async Task<ResponseDataParameterResolve> Handle(QueryDataParameterTextResolve request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityDataParameterText? result = await uow.Data.GetCurrentTextParameterByIdentifier(request.SchemaIdentifier);

        return new() { Value = result?.Value ?? "[MISSING]" };
    }
}
