using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Data;

[Table(nameof(TableDataElement))]
public class TableDataElement : BaseTableData
{
    public ICollection<TableDataProperty>? PropertyList { get; set; }
}
