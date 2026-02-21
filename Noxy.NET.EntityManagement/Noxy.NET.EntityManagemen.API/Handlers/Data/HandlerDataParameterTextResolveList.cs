using MediatR;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterTextResolveList(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QueryDataParameterTextResolveList, ResponseDataParameterResolveList>
{
    public async Task<ResponseDataParameterResolveList> Handle(QueryDataParameterTextResolveList request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Dictionary<string, EntityDataParameterText?> results = await uow.Data.GetCurrentTextParameterByIdentifierList(request.SchemaIdentifierList);
        Dictionary<string, string> resultDict = results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value is { } v ? v.Value : "[MISSING]");

        return new() { Value = resultDict };
    }
}
