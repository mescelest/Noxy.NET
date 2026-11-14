using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data;

public class TableDataPropertyString : TableDataProperty
{
    [MaxLength(DefaultTextLength)]
    public required string? Value { get; set; }
}
