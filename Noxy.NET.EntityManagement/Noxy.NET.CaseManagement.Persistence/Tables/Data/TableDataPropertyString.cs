using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.CaseManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Data;

[Table(nameof(TableDataPropertyString))]
public class TableDataPropertyString : TableDataProperty
{
    public required string? Value { get; set; }
}
