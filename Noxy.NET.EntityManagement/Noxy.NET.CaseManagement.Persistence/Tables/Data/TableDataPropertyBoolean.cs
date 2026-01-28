using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.CaseManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Data;

[Table(nameof(TableDataPropertyBoolean))]
public class TableDataPropertyBoolean : TableDataProperty
{
    public required bool? Value { get; set; }
}
