using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.CaseManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Data;

[Table(nameof(TableDataPropertyImage))]
public class TableDataPropertyImage : TableDataProperty
{
    public required string Value { get; set; }
}
