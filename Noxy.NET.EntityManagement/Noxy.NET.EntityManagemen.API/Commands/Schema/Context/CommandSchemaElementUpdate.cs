using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Context;

public class CommandSchemaContextUpdate(Guid id, RequestSchemaContextUpdate request) : IRequest<ResponseSchemaContextUpdate>
{
    public Guid ID { get; } = id;
    public string SchemaIdentifier { get; } = request.SchemaIdentifier;
    public string Name { get; } = request.Name;
    public string Note { get; } = request.Note;
    public Guid TitleParameterTextID { get; } = request.TitleParameterTextID;
    public Guid? DescriptionParameterTextID { get; } = request.DescriptionParameterTextID;
}
