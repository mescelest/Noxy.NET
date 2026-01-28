using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data;

[Table(nameof(TableDataPropertyInteger))]
public class TableDataPropertyInteger : TableDataProperty
{
    public required int? Value { get; set; }
}
