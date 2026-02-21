using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyDateTime : TableSchemaProperty.Primitive
{
    [Required]
    public required DateTimeTypeEnum Type { get; set; }
}
