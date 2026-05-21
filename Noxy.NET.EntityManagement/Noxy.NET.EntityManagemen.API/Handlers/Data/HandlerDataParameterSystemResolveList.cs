using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Data;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterSystemResolveList(IParameterService serviceParameter) : IQueryHandler<QueryDataParameterSystemResolveList, ResponseDataParameterSystemResolveList>
{
    public ValueTask<ResponseDataParameterSystemResolveList> Handle(QueryDataParameterSystemResolveList request, CancellationToken cancellationToken)
    {
        Dictionary<string, string?> result = request.SchemaIdentifierList.ToDictionary(x => x, y => serviceParameter.TryGetParameterSystem(y, out EntityDataParameterSystem? parameter) ? parameter.Value : null);
        return ValueTask.FromResult(new ResponseDataParameterSystemResolveList(result));
    }
}
