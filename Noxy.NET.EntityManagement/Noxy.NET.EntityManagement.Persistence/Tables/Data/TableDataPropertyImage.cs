using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data;

[Table(nameof(TableDataPropertyImage))]
public class TableDataPropertyImage : TableDataProperty
{
    public required string Value { get; set; }
}
