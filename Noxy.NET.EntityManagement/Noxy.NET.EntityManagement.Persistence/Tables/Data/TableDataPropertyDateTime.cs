using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data;

[Table(nameof(TableDataPropertyDateTime))]
public class TableDataPropertyDateTime : TableDataProperty
{
    public required DateTime? Value { get; set; }
}
