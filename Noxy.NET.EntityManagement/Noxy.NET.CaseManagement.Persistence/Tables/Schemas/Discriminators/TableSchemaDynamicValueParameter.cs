using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;

public abstract class TableSchemaDynamicValueParameter : TableSchemaDynamicValue
{
    [Required]
    public required bool IsApprovalRequired { get; set; } 
}
