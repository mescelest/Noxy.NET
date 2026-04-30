using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;

public class CommandSchemaParameterDelete(Guid id) : IRequest<ResponseSchemaParameterDelete>
{
    public Guid ID { get; } = id;
}
