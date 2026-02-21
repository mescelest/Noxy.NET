using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyImage : TableSchemaProperty.Primitive
{
    [Required]
    public required string AllowedExtensions { get; set; }
}
