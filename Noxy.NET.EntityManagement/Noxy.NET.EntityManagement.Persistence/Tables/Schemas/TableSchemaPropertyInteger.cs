using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyInteger : TableSchemaProperty
{
    public bool IsUnsigned { get; set; }
}
