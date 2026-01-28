using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

public class FormModelSchema(EntitySchema? entity = null) : BaseFormModelEntityTemplate(entity)
{
    [JsonConstructor]
    public FormModelSchema() : this(null)
    {
    }

    public override string APIEndpoint => "Template/Schema";
    public override HttpMethod HttpMethod => HttpMethod.Post;
}
