using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Element;

public class CommandSchemaElementClone(Guid id) : IRequest<ResponseSchemaElementClone>
{
    public Guid ID { get; } = id;
}
