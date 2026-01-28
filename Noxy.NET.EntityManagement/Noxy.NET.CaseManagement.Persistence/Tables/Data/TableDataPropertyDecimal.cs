using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.CaseManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Data;

[Table(nameof(TableDataPropertyDecimal))]
public class TableDataPropertyDecimal : TableDataProperty
{
    public required decimal? Value { get; set; }
}
