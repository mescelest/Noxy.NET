using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

public abstract class TableSchemaDynamicValueParameter : TableSchemaDynamicValue
{
    [Required]
    public required bool IsApprovalRequired { get; set; }
}
