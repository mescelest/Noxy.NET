using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

public abstract class TableSchemaParameter : BaseTableSchema
{
    [Required]
    public required bool IsApprovalRequired { get; set; }
}
