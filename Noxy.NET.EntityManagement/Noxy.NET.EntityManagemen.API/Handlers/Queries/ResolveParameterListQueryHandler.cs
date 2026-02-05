using MediatR;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Queries;

public class ResolveTextParameterListQueryHandler(IUnitOfWorkFactory unitOfWorkFactory) : IRequestHandler<ResolveTextParameterListQuery, Dictionary<string, string>>
{
    public async Task<Dictionary<string, string>> Handle(ResolveTextParameterListQuery request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await unitOfWorkFactory.Create();

        Dictionary<string, EntityDataParameterText?> results = await uow.Data.GetCurrentTextParameterByIdentifierList(request.ParameterKeys);
        Dictionary<string, string> resultDict = results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value is { } v ? v.Value : "[MISSING]");

        return resultDict;
    }
}
