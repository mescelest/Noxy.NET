using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data;

[Table(nameof(TableDataPropertyBoolean))]
public class TableDataPropertyBoolean : TableDataProperty
{
    public required bool? Value { get; set; }
}
