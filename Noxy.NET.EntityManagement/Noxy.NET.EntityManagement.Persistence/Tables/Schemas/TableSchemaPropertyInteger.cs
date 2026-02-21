using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyInteger : TableSchemaProperty.Primitive
{
    public bool IsUnsigned { get; set; }
}
