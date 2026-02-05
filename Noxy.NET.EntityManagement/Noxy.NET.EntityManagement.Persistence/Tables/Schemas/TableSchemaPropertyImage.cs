using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaPropertyImage))]
public class TableSchemaPropertyImage : TableSchemaProperty.Primitive
{
    [Required]
    public required string AllowedExtensions { get; set; }
}
