using Mediator;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;

public class CommandSchemaParameterStyleCreate(RequestSchemaParameterStyleCreate request) : ICommand<ResponseSchemaParameterStyleCreate>
{
    public Guid? SchemaID { get; } = request.SchemaID;
    public string SchemaIdentifier { get; } = request.SchemaIdentifier;
    public string Name { get; } = request.Name;
    public string Note { get; } = request.Note;
    public bool IsSystemDefined { get; } = request.IsSystemDefined;
    public bool IsApprovalRequired { get; } = request.IsApprovalRequired;
}
