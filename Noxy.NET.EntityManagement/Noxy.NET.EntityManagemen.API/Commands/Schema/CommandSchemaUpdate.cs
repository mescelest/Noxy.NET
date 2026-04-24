using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Commands.Schema;

public class CommandSchemaUpdate(Guid id, RequestSchemaUpdate request) : IRequest<ResponseSchemaUpdate>
{
    public Guid ID { get; } = id;
    public string Name { get; } = request.Name;
    public string Note { get; } = request.Note;
}
