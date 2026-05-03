using MediatR;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Property;

public class CommandSchemaPropertyDateTimeUpdate(Guid id, RequestSchemaPropertyDateTimeUpdate request) : IRequest<ResponseSchemaPropertyDateTimeUpdate>
{
    public Guid ID { get; } = id;
    public string SchemaIdentifier { get; } = request.SchemaIdentifier;
    public string Name { get; } = request.Name;
    public string Note { get; } = request.Note;
    public DateTimeTypeEnum Type { get; } = request.Type;
    public int? Weight { get; } = request.Weight;
    public Guid TitleParameterTextID { get; } = request.TitleParameterTextID;
    public Guid? DescriptionParameterTextID { get; } = request.DescriptionParameterTextID;
}
