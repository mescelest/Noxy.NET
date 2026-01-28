using System.ComponentModel.DataAnnotations;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas.Associations;

public class EntityAssociationSchemaActionInputHasAttributeInteger : EntityAssociationSchemaActionInputHasAttribute
{
    [Required]
    public required int? Value { get; set; }
}
