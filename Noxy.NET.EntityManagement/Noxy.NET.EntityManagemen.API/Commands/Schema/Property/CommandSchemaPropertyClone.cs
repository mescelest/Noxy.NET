using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Property;

public class CommandSchemaPropertyClone(Guid id) : IRequest<ResponseSchemaPropertyClone>
{
    public Guid ID { get; } = id;
}
