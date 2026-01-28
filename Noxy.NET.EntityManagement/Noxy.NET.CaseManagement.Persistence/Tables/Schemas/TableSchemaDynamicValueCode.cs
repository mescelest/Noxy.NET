using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaDynamicValueCode))]
[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public class TableSchemaDynamicValueCode : TableSchemaDynamicValue
{
    [Required]
    public required string Value { get; set; }
    
    [Required]
    public required bool IsAsynchronous { get; set; }
}
