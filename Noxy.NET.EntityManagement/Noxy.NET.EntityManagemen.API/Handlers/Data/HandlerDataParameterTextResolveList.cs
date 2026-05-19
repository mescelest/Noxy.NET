using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Data;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterTextResolveList(IParameterService serviceParameter) : IQueryHandler<QueryDataParameterTextResolveList, ResponseDataParameterTextResolveList>
{
    public ValueTask<ResponseDataParameterTextResolveList> Handle(QueryDataParameterTextResolveList request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new ResponseDataParameterTextResolveList
        {
            Value = request.SchemaIdentifierList.ToDictionary(x => x, y => serviceParameter.TryGetParameterText(y, out EntityDataParameterText? parameter) ? parameter.Value : null)
        });
    }
}
