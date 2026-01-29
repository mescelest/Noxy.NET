using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaParameterText))]
[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public class TableSchemaParameterText : TableSchemaParameter
{
    [Required]
    public required TextParameterTypeEnum Type { get; set; }
}
