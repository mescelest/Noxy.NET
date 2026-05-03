using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.ElementHasProperty;

public class RequestSchemaElementHasPropertyCreate : BaseRequestPost<ResponseSchemaElementHasPropertyCreate>
{
    public override string APIEndpoint => "Schema/Element/Property";

    [NotEmptyGuid]
    public Guid SchemaElementID { get; set; }

    [NotEmptyGuid]
    public Guid SchemaPropertyID { get; set; }
}
