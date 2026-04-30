using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;

public class CommandSchemaParameterClone(Guid id) : IRequest<ResponseSchemaParameterClone>
{
    public Guid ID { get; } = id;
}
