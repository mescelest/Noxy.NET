using Mediator;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Commands.Schema;

public class CommandSchemaCreate(RequestSchemaCreate request) : ICommand<ResponseSchemaCreate>
{
    public string Name { get; } = request.Name;
    public string Note { get; } = request.Note;
}
