using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchema(EntitySchema? entity = null) : BaseFormModelEntityTemplate(entity)
{
    public override string APIEndpoint => "Template/Schema";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [JsonConstructor]
    public FormModelSchema() : this(null)
    {
    }
}
