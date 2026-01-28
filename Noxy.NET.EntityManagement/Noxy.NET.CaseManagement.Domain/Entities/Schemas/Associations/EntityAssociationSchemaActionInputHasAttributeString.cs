using System.ComponentModel.DataAnnotations;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas.Associations;

public class EntityAssociationSchemaActionInputHasAttributeString : EntityAssociationSchemaActionInputHasAttribute
{
    [Required]
    public required string Value { get; set; }
}
