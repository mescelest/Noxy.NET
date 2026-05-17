using Mediator;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;

public class CommandSchemaParameterSystemUpdate(Guid id, RequestSchemaParameterSystemUpdate request) : ICommand<ResponseSchemaParameterSystemUpdate>
{
    public Guid ID { get; } = id;
    public string SchemaIdentifier { get; } = request.SchemaIdentifier;
    public string Name { get; } = request.Name;
    public string Note { get; } = request.Note;
    public ParameterSystemTypeEnum Type { get; } = request.Type;
    public bool IsSystemDefined { get; } = request.IsSystemDefined;
    public bool IsApprovalRequired { get; } = request.IsApprovalRequired;
}
