using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaPropertyInteger))]
public class TableSchemaPropertyInteger : TableSchemaProperty.Primitive
{
    public bool IsUnsigned { get; set; } = false;
}
