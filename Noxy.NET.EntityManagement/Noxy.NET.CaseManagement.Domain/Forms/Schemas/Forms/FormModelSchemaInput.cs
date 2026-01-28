using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaInput(EntitySchemaInput? entity = null) : BaseFormModelEntitySchema(entity)
{
    public override string APIEndpoint => "Schema/Input";
    public override HttpMethod HttpMethod => HttpMethod.Post;
    
    [JsonConstructor]
    public FormModelSchemaInput() : this(null)
    {
    }
}
