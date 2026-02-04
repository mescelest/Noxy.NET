using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas;

public class FormModelSchemaPropertyDecimal(EntitySchemaPropertyDecimal? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    [JsonConstructor]
    public FormModelSchemaPropertyDecimal() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/Property/Decimal";
    public override HttpMethod HttpMethod => HttpMethod.Post;
}
