using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Enums;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas;

public class FormModelSchemaPropertyDateTime(EntitySchemaPropertyDateTime? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    [JsonConstructor]
    public FormModelSchemaPropertyDateTime() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/Property/DateTime";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    public DateTimeTypeEnum Type { get; set; } = entity?.Type ?? DateTimeTypeEnum.Date;
}
