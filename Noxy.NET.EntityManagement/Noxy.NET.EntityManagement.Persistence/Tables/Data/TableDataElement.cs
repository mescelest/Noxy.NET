using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data;

[Table(nameof(TableDataElement))]
public class TableDataElement : BaseTableData
{
    public ICollection<TableDataProperty>? PropertyList { get; set; }
}
