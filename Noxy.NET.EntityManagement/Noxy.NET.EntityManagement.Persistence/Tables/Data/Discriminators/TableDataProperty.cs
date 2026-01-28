using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

public abstract class TableDataProperty : BaseTableData
{
    [Required]
    public TableDataElement? Element { get; set; }
    public required Guid ElementID { get; set; }
}
