using MediatR;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Handlers;

public class HandlerDataParameterTextResolve(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QueryDataParameterTextResolve, ResponseDataParameterTextResolve>
{
    public async Task<ResponseDataParameterTextResolve> Handle(QueryDataParameterTextResolve request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityDataParameterText? result = await uow.Data.GetCurrentTextParameterByIdentifier(request.Identifier);

        return new() { Value = result?.Value ?? "[MISSING]" };
    }
}
